using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Treasured.UnitySdk
{
    [Flags]
    public enum ExportOptions
    {
        JSON = 1 << 0,
        Cubemap = 1 << 1,
        Mask = 1 << 2,
        All = JSON | Cubemap | Mask
    }

    internal class TreasuredMapExporter : IDisposable
    {
        /// <summary>
        /// Default output folder in project root
        /// </summary>
        public const string DefaultOutputFolder = "Treasured Data/";
        public static readonly string DefaultOutputFolderPath = $"{Directory.GetCurrentDirectory()}/{DefaultOutputFolder}";

        private static readonly int[] builtInLayers = new int[] { 0, 1, 2, 4, 5 };
       
        #region Image

        private static Material objectIdConverter;
        private static int colorId;
        #endregion

        private TreasuredMap _target;
        private SerializedObject _serializedObject;

        private DirectoryInfo _outputDirectory;

        private SerializedProperty _interactableLayer;

        private ExportOptions exportOptions = ExportOptions.All;
        private CubemapFormat cubemapFormat;
        private int qualityPercentage = 75;
        private bool overwriteExistingData = true;

        public TreasuredMapExporter(SerializedObject serializedObject, TreasuredMap map)
        {
            this._target = map;
            this._serializedObject = serializedObject;
        }

        private void ValidateOutputDirectory()
        {
            if (_target == null)
            {
                throw new NullReferenceException("Map is not assigned for exporter.");
            }
            try
            {
                _outputDirectory = Directory.CreateDirectory($"{DefaultOutputFolderPath}/{_target.OutputFolderName}");
            }
            catch (Exception ex) when (ex is IOException || ex is ArgumentException)
            {
                throw new ArgumentException($"Invalid folder name : {_target.OutputFolderName}");
            }
            catch
            {
                throw;
            }
        }

        private void ExportJson()
        {
            
        }

        private void ExportCubemap()
        {
            
        }

        private void ExportMask()
        {
            var hotspots = _target.Hotspots;// ValidateHotspots();
            int interactableLayer = _serializedObject.FindProperty("_interactableLayer").intValue; // single layer
            Color maskColor = _target.MaskColor;
            Color backgroundColor = Color.black;

            Dictionary<Renderer, Material> defaultMaterials = new Dictionary<Renderer, Material>();
            Dictionary<Renderer, int> defaultLayers = new Dictionary<Renderer, int>();
            List<GameObject> tempHotspots = new List<GameObject>();
            var cameraGO = new GameObject("Cubemap Camera"); // creates a temporary camera with some default settings.
            cameraGO.hideFlags = HideFlags.DontSave;
            var camera = cameraGO.AddComponent<Camera>();
            var cameraData = cameraGO.AddComponent<HDAdditionalCameraData>();
            if (cameraData == null)
            {
                throw new MissingComponentException("Missing HDAdditionalCameraData component");
            }
            camera.cullingMask = 1 << interactableLayer;
            try
            {
                TreasuredObject[] objects = _target.GetComponentsInChildren<TreasuredObject>();
                if (objectIdConverter == null)
                {
                    objectIdConverter = new Material(Shader.Find("Hidden/ObjectId"));
                }
                

                #region HDRP camera settings
                cameraData.backgroundColorHDR = backgroundColor;
                cameraData.clearColorMode = HDAdditionalCameraData.ClearColorMode.Color;
                cameraData.volumeLayerMask = 0; // ensure no volume effects will affect the object id color
                cameraData.probeLayerMask = 0; // ensure no probe effects will affect the object id color
                #endregion
                int size = Mathf.Min(Mathf.NextPowerOfTwo((int)_target.Quality), 8192);
                Cubemap cubemap = new Cubemap(size * 6, TextureFormat.ARGB32, false);
                Texture2D texture = new Texture2D(cubemap.width * (cubemapFormat == CubemapFormat.Single ? 6 : 1), cubemap.height, TextureFormat.ARGB32, false);

                // Create tempory hotspot object
                foreach (var hotspot in _target.Hotspots)
                {
                    GameObject tempGO = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    tempGO.hideFlags = HideFlags.HideAndDontSave;
                    tempGO.transform.SetParent(hotspot.Hitbox.transform);
                    tempGO.transform.localScale = new Vector3(0.5f, 0.01f, 0.5f);
                    tempGO.transform.localPosition = Vector3.zero;
                    tempGO.layer = interactableLayer;
                    tempHotspots.Add(tempGO);
                }

                // Set Object Color
                for (int i = 0; i < objects.Length; i++)
                {
                    TreasuredObject obj = objects[i];
                    Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
                    MaterialPropertyBlock mpb = new MaterialPropertyBlock();
                    foreach (var renderer in renderers)
                    {
                        defaultLayers[renderer] = renderer.gameObject.layer;
                        renderer.gameObject.layer = interactableLayer;
                        defaultMaterials[renderer] = renderer.sharedMaterial;
                        renderer.sharedMaterial = objectIdConverter;
                        renderer.GetPropertyBlock(mpb);
                        mpb.SetColor("_IdColor", maskColor);
                        renderer.SetPropertyBlock(mpb);
                    }
                }


                for (int index = 0; index < hotspots.Length; index++)
                {
                    Hotspot current = hotspots[index];
                    string progressTitle = $"Exporting Mask ({index + 1}/{hotspots.Length})";
                    string progressText = $"Generating data for {current.name}...";
                    current.gameObject.SetActive(true);
                    foreach (var obj in objects)
                    {
                        obj.gameObject.SetActive(true);
                    }
                    var visibleTargets = current.VisibleTargets;
                    foreach (var invisibleTarget in objects.Except(visibleTargets))
                    {
                        invisibleTarget.gameObject.SetActive(false);
                    }
                    camera.transform.SetPositionAndRotation(current.Camera.transform.position, Quaternion.identity);

                    if (!camera.RenderToCubemap(cubemap))
                    {
                        throw new System.NotSupportedException("Current graphic device/platform does not support RenderToCubemap.");
                    }
                    var di = CreateDirectory(DefaultOutputFolderPath, _target.OutputFolderName, "images", current.Id);
                    for (int i = 0; i < 6; i++)
                    {
                        DisplayCancelableProgressBar(progressTitle, progressText, i / 6f);
                        texture.SetPixels(cubemap.GetPixels((CubemapFace)i));
                        ImageUtilies.FlipPixels(texture, true, true);
                        ImageUtilies.Encode(texture, di.FullName, "mask_" + SimplifyCubemapFace((CubemapFace)i), ImageFormat.WEBP, qualityPercentage);
                    }
                }

            }
            finally
            {
                foreach (var kvp in defaultMaterials)
                {
                    kvp.Key.sharedMaterial = kvp.Value;
                }
                // Restore layer
                foreach (var kvp in defaultLayers)
                {
                    kvp.Key.gameObject.layer = kvp.Value;
                }

                foreach (var tempHotspot in tempHotspots)
                {
                    GameObject.DestroyImmediate(tempHotspot);
                }
                if (cameraGO != null)
                {
                    GameObject.DestroyImmediate(cameraGO);
                    cameraGO = null;
                }
            }
        }

        private void DisplayCancelableProgressBar(string title, string info, float progress)
        {
            if (EditorUtility.DisplayCancelableProgressBar(title, info, progress))
            {
                throw new TreasuredException("Export canceled", "Export canceled by the user.");
            }
        }

        private string SimplifyCubemapFace(CubemapFace cubemapFace)
        {
            switch (cubemapFace)
            {
                case CubemapFace.PositiveX:
                    return "px";
                case CubemapFace.NegativeX:
                    return "nx";
                case CubemapFace.PositiveY:
                    return "py";
                case CubemapFace.NegativeY:
                    return "ny";
                case CubemapFace.PositiveZ:
                    return "pz";
                case CubemapFace.NegativeZ:
                    return "nz";
                case CubemapFace.Unknown:
                default:
                    return "unknown";
            }
        }

        public void Export(ExportOptions options)
        {
            if (builtInLayers.Contains(_serializedObject.FindProperty(nameof(_interactableLayer)).intValue))
            {
                Debug.LogError("Can not use a built-in layer as the Interactable Layer.");
                return;
            }
            try
            {
                string directoryPath = $"{DefaultOutputFolderPath}/{_target.OutputFolderName}";
                if (overwriteExistingData && Directory.Exists(directoryPath))
                {
                    Directory.Delete(directoryPath, true);
                }
                ValidateOutputDirectory();
                if (options.HasFlag(ExportOptions.JSON))
                {
                    ExportJson();
                }
                if (options.HasFlag(ExportOptions.Cubemap))
                {
                    ExportCubemap();
                }
                if (options.HasFlag(ExportOptions.Mask))
                {
                    ExportMask();
                }
            }
            catch (ContextException e)
            {
                if (EditorUtility.DisplayDialog(e.Title, e.Message, e.PingText))
                {
                    EditorGUIUtility.PingObject(e.Context);
                }
            }
            catch (TreasuredException e)
            {
                EditorUtility.DisplayDialog(e.Title, e.Message, "Ok");
            }
            catch (Exception e)
            {
                string exceptionType = e.GetType().Name.ToString();
                if (exceptionType.EndsWith("Exception"))
                {
                    exceptionType = exceptionType.Substring(0, exceptionType.LastIndexOf("Exception"));
                }
                EditorUtility.DisplayDialog(ObjectNames.NicifyVariableName(exceptionType), e.Message, "Ok");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                Dispose();
            }
        }

        private void OnExportOptionsGUI()
        {
            EditorGUILayout.LabelField("Export Options");
            EditorGUI.indentLevel++;
            foreach (ExportOptions option in Enum.GetValues(typeof(ExportOptions)))
            {
                bool hasFlag = exportOptions.HasFlag(option);
                EditorGUI.BeginChangeCheck();
                var enable = EditorGUILayout.Toggle(ObjectNames.NicifyVariableName(option.ToString()), hasFlag);
                if (EditorGUI.EndChangeCheck())
                {
                    if (!enable)
                    {
                        exportOptions &= ~option;
                    }
                    else
                    {
                        exportOptions |= option;
                    }
                }
            }
            EditorGUI.indentLevel--;
        }

        

        private DirectoryInfo CreateDirectory(params string[] paths)
        {
            return Directory.CreateDirectory(Path.Combine(paths));
        }

        public void Dispose()
        {
            if (objectIdConverter != null)
            {
                GameObject.DestroyImmediate(objectIdConverter);
                objectIdConverter = null;
            }
        }
    }
}
