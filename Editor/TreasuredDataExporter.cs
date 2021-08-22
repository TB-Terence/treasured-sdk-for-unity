using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
using Treasured.SDK;
using System.IO;
using UnityEngine.Rendering;
using System;
using UnityEditor.SceneManagement;
using Simple360Render;
using UnityEditorInternal;
using System.Collections.Generic;
using System.Linq;

namespace Treasured.SDKEditor
{
    internal partial class TreasuredDataEditor
    {
        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ContractResolver = CustomContractResolver.Instance
        };

        private static Camera _camera;
        private static RenderTexture _cubeMapTexture;
        private static RenderTexture _equirectangularTexture;
        private static Texture2D _outputTexture;
        private static Material _equirectangularConverter;
        private static int _paddingX;

        bool Export(int layerMask)
        {
            string directory = EditorUtility.SaveFolderPanel("Choose output directory", Path.GetDirectoryName(Application.dataPath), "");
            if (!string.IsNullOrEmpty(directory))
            {
                string folderName = string.IsNullOrEmpty(_data.Name) ? EditorSceneManager.GetActiveScene().name : _data.Name;
                directory = Path.Combine(directory, $"{folderName}/");
                ExportPanorama(directory, layerMask);
                ExportJson(directory);
                Application.OpenURL(directory);
                return true;
            }
            return false;
        }

        void ExportPanorama(string directory, int layerMask)
        {
            if (_camera == null)
            {
                _camera = Camera.main;
                if (_camera == null)
                {
                    Debug.LogError("No active camera found in scene.");
                    return;
                }
            }
            #region Get current camera/RenderTexture settings
            RenderTexture camTarget = _camera.targetTexture;
            Vector3 originalCameraPos = _camera.transform.position;
            Quaternion originalCameraRot = _camera.transform.rotation;
            RenderTexture activeRT = RenderTexture.active;
            #endregion

            #region Create output directories
            string qualityFolderName = $"{Enum.GetName(typeof(ImageQuality), _data.Quality).ToLower()}/";
            string qualityFolderDirectory = Path.Combine(directory, qualityFolderName);
            if (Directory.Exists(qualityFolderDirectory))
            {
                DirectoryInfo info = new DirectoryInfo(qualityFolderDirectory);
                FileInfo[] fileInfos = info.GetFiles();
                if (info.GetFiles().Length > 0)
                {
                    IEnumerable<string> files = fileInfos.Select(x => Path.GetFileNameWithoutExtension(x.Name)).Except(_data.Hotspots.Select(x => x.Id));
                    foreach (var file in files)
                    {
                        Debug.LogWarning($"{qualityFolderDirectory} contains file '{file}' which is not part of the data.");
                    }
                }
            }
            else
            {
                Directory.CreateDirectory(qualityFolderDirectory);
            }
            #endregion

            #region Settings
            bool encodeAsJPEG = _data.Format == ImageFormat.JPG;
            int cubeMapSize = Mathf.Min(Mathf.NextPowerOfTwo((int)_data.Quality), 8192);
            int count = _data.Hotspots.Count;
            bool faceCameraDirection = true;
            #endregion

            #region Create RenderTexture/Texture2D
            _cubeMapTexture = RenderTexture.GetTemporary(cubeMapSize, cubeMapSize, 0);
            _cubeMapTexture.dimension = TextureDimension.Cube;
            _equirectangularTexture = RenderTexture.GetTemporary(cubeMapSize, cubeMapSize / 2, 0);
            _equirectangularTexture.dimension = TextureDimension.Tex2D;
            _outputTexture = new Texture2D(_equirectangularTexture.width, _equirectangularTexture.height, TextureFormat.RGB24, false);
            #endregion

            if (_equirectangularConverter == null)
            {
                _equirectangularConverter = new Material(Shader.Find("Hidden/I360CubemapToEquirectangular"));
                _paddingX = Shader.PropertyToID("_PaddingX");
            }

            try
            {
                IEnumerable<TreasuredObject> targets = _data.All;
                for (int index = 0; index < count; index++)
                {
                    TreasuredObject hotspot = _data.Hotspots[index];
                    // Calculate Visible Targets
                    hotspot.ResetVisibleTargets();
                    foreach (var target in targets)
                    {
                        if (target.Id == hotspot.Id)
                        {
                            continue;
                        }
                        if (!Physics.Linecast(hotspot.Transform.Position, target.Transform.Position, layerMask))
                        {
                            hotspot.AddVisibleTarget(target);
                        }
                    }

                    var fileName = $"{hotspot.Id}.{_data.Format.ToString().ToLower()}";
                    // Move the camera in the right position
                    _camera.transform.SetPositionAndRotation(hotspot.Transform.Position, Quaternion.identity);
                    
                    EditorUtility.DisplayProgressBar($"Exporting image ({index + 1}/{count})", $"Working on cubemap for {hotspot.Name}", 0.33f);
                    if (!_camera.RenderToCubemap(_cubeMapTexture, 63))
                    {
                        throw new NotSupportedException("Rendering to cubemap is not supported on device/platform!");
                    }

                    EditorUtility.DisplayProgressBar($"Exporting image ({index + 1}/{count})", "Applying shader...", 0.66f);
                    _equirectangularConverter.SetFloat(_paddingX, faceCameraDirection ? (_camera.transform.eulerAngles.y / 360f) : 0f);
                    Graphics.Blit(_cubeMapTexture, _equirectangularTexture, _equirectangularConverter);

                    RenderTexture.active = _equirectangularTexture;
                    _outputTexture.ReadPixels(new Rect(0, 0, _equirectangularTexture.width, _equirectangularTexture.height), 0, 0);
                    
                    EditorUtility.DisplayProgressBar($"Exporting image ({index + 1}/{count})", $"Inserting XMP Data for {fileName}...", 0.99f);
                    byte[] bytes = encodeAsJPEG ? I360Render.InsertXMPIntoTexture2D_JPEG(_outputTexture) : I360Render.InsertXMPIntoTexture2D_PNG(_outputTexture);
                    if (bytes != null)
                    {
                        EditorUtility.DisplayProgressBar($"Exporting {fileName} ({index + 1}/{count})", $"Saving {fileName}...", 0.99f);
                        string path = Path.Combine(qualityFolderDirectory, fileName);
                        File.WriteAllBytes(path, bytes);
                    }
                }
            }
            catch(Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                #region Restore settings
                _camera.transform.position = originalCameraPos;
                _camera.transform.rotation = originalCameraRot;
                _camera.targetTexture = camTarget;
                RenderTexture.active = activeRT;
                #endregion

                #region Free resources
                if (_cubeMapTexture != null)
                    RenderTexture.ReleaseTemporary(_cubeMapTexture);

                if (_equirectangularTexture != null)
                    RenderTexture.ReleaseTemporary(_equirectangularTexture);

                if (_outputTexture != null)
                    DestroyImmediate(_outputTexture);

                if (_equirectangularConverter != null)
                    DestroyImmediate(_equirectangularConverter);
                #endregion
            }
        }

        void ExportJson(string directory)
        {
            string jsonPath = $"{directory}/data.json";
            string json = JsonConvert.SerializeObject(target, Formatting.Indented, JsonSettings);
            File.WriteAllText(jsonPath, json);
        }

        void ShowExportConfigWindow(Rect rect)
        {
            PopupWindow.Show(rect, new ExportConfigWindow(this));
        }

        private class ExportConfigWindow : PopupWindowContent
        {
            public static readonly Vector2 WindowSize = new Vector2(270, 64);

            private TreasuredDataEditor _editor;
            private bool _showInExplorer = true;
            private int _layerMask;

            private GUIContent buttonStart = new GUIContent("Start");

            public ExportConfigWindow(TreasuredDataEditor editor)
            {
                _editor = editor;
            }

            public override Vector2 GetWindowSize()
            {
                return WindowSize;
            }

            public override void OnGUI(Rect rect)
            {
                _showInExplorer = EditorGUI.Toggle(new Rect(2, 2, rect.width, 20), "Show In Explorer", _showInExplorer);
                _layerMask = EditorGUI.MaskField(new Rect(2, 22, rect.width, 20), new GUIContent("Interactable Layer"), _layerMask, InternalEditorUtility.layers);
                Rect buttonRect = GUILayoutUtility.GetRect(buttonStart, GUI.skin.button);
                if (GUI.Button(new Rect(rect.xMax - buttonRect.width - 4, rect.height - buttonRect.height - 4, rect.width - 4, 20), buttonStart))
                {
                    try
                    {
                        _editor.Export(_layerMask);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }
                }
            }
        }
    }
}
