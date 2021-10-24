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
        PanoramicImages = 1 << 1,
        Mask = 1 << 2,
        All = JSON | PanoramicImages | Mask
    }

    internal class TreasuredMapExporter : IDisposable
    {
        /// <summary>
        /// Default output folder in project root
        /// </summary>
        public const string DefaultOutputFolder = "Treasured Data/";
        public static readonly string DefaultOutputFolderPath = $"{Directory.GetCurrentDirectory()}/{DefaultOutputFolder}";

        private static readonly int[] builtInLayers = new int[] { 0, 1, 2, 4, 5 };
        static class Styles
        {
            public static readonly GUIContent folderOpened = EditorGUIUtility.TrIconContent("FolderOpened Icon", "Show in Explorer");
        }

        #region Json
        public static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ContractResolver = ContractResolver.Instance,
            CheckAdditionalContent = true
        };
        #endregion

        #region Image

        private static Material objectIdConverter;
        private static int colorId;
        #endregion

        private TreasuredMap _target;
        private SerializedObject _serializedObject;

        private DirectoryInfo _outputDirectory;


        private int _paddingXId;

        private RenderTexture _cubemapRT;
        private RenderTexture _equirectangularRT;
        private Texture2D _outputTexture;
        private Material _equirectangularConverter;

        private SerializedProperty _outputFolderName;
        private SerializedProperty _interactableLayer;

        private ExportOptions exportOptions = ExportOptions.All;
        private CubemapFormat cubemapFormat = CubemapFormat.Six;
        private int compression = 75;

        public TreasuredMapExporter(SerializedObject serializedObject, TreasuredMap map)
        {
            this._target = map;
            this._serializedObject = serializedObject;

            this._outputFolderName = serializedObject.FindProperty(nameof(_outputFolderName));
            if (string.IsNullOrEmpty(_outputFolderName.stringValue))
            {
                _outputFolderName.stringValue = EditorSceneManager.GetActiveScene().name;
                serializedObject.ApplyModifiedProperties();
            }
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
            JsonValidator.ValidateMap(_target);
            ValidateOutputDirectory();
            string jsonPath = Path.Combine(_outputDirectory.FullName, "data.json");
            string json = JsonConvert.SerializeObject(_target, Formatting.Indented, JsonSettings);
            File.WriteAllText(jsonPath, json);
        }

        private void ExportPanoramicImages()
        {
            var hotspots = _target.Hotspots;
            if (hotspots == null || hotspots.Length == 0)
            {
                throw new InvalidOperationException("No active hotspots.");
            }
            ValidateOutputDirectory();
            var camera = ValidateCamera(); // use default camera settings to render 360 images

            #region Get camera settings
            RenderTexture camTarget = camera.targetTexture;
            Vector3 originalCameraPos = camera.transform.position;
            Quaternion originalCameraRot = camera.transform.rotation;
            RenderTexture activeRT = RenderTexture.active;
            #endregion

            string fileName = _target.Quality.ToString().ToLower();

            string extension = _target.Format.ToString().ToLower();

            int size = Mathf.Min(Mathf.NextPowerOfTwo((int)_target.Quality), 8192);

            int count = hotspots.Length;

            Cubemap cubemap = new Cubemap(size, TextureFormat.ARGB32, false);
            Texture2D texture = new Texture2D(cubemap.width * (cubemapFormat == CubemapFormat.Single ? 6 : 1), cubemap.height, TextureFormat.ARGB32, false);

            try
            {
                for (int index = 0; index < count; index++)
                {
                    Hotspot current = hotspots[index];
                    string progressTitle = $"Exporting Panoramic Image ({index + 1}/{count})";
                    string progressText = $"Generating data for {current.name}...";
                    EditorUtility.DisplayProgressBar(progressTitle, progressText, 0.33f);

                    camera.transform.SetPositionAndRotation(current.Camera.transform.position, Quaternion.identity);

                    if (!camera.RenderToCubemap(cubemap))
                    {
                        throw new System.NotSupportedException("Current graphic device/platform does not support RenderToCubemap.");
                    }
                    var di = CreateDirectory(DefaultOutputFolderPath, _target.OutputFolderName, "images", current.Id);
                    switch (cubemapFormat)
                    {
                        case CubemapFormat.Single:
                            for (int i = 0; i < 6; i++)
                            {
                                EditorUtility.DisplayProgressBar(progressTitle, progressText, 0.33f + i * 0.11f);
                                texture.SetPixels(i * size, 0, size, size, cubemap.GetPixels((CubemapFace)i));
                            }
                            FlipPixels(texture, true, true);
                            ImageEncoder.Encode(texture, di.FullName, "cubemap", _target.Format, compression);
                            break;
                        case CubemapFormat.Six:
                            for (int i = 0; i < 6; i++)
                            {
                                EditorUtility.DisplayProgressBar(progressTitle, progressText, 0.33f + i * 0.11f);
                                texture.SetPixels(cubemap.GetPixels((CubemapFace)i));
                                FlipPixels(texture, true, true);
                                ImageEncoder.Encode(texture, di.FullName, SimplifyCubemapFace((CubemapFace)i), _target.Format, compression);
                            }
                            break;
                    }
                }
            }
            catch(Exception e)
            {
                throw e;
            }
            finally
            {
                GameObject.DestroyImmediate(cubemap);
                GameObject.DestroyImmediate(texture);
                #region Restore settings
                camera.transform.position = originalCameraPos;
                camera.transform.rotation = originalCameraRot;
                camera.targetTexture = camTarget;
                RenderTexture.active = activeRT;
                #endregion
            }
        }

        private void ExportMask()
        {
            ValidateOutputDirectory();

            TreasuredObject[] objects = _target.GetComponentsInChildren<TreasuredObject>();
            //Color[] objectIds = objects.Select(x => x.ObjectId).ToArray();
            Color maskColor = _target.MaskColor;
            Color backgroundColor = Color.black;
            Dictionary<Renderer, Material> defaultMaterials = new Dictionary<Renderer, Material>();
            Dictionary<Renderer, int> defaultLayers = new Dictionary<Renderer, int>(); // doesn't seem necessary but it will prevent breaking existing objects.
            int interactableLayer = _serializedObject.FindProperty("_interactableLayer").intValue; // single layer

            RenderTexture activeRT = RenderTexture.active;

            var cameraGO = new GameObject("Panoramic Image Camera"); // creates a temporary camera with some default settings.
            cameraGO.hideFlags = HideFlags.DontSave;
            var camera = cameraGO.AddComponent<Camera>();
            var cameraData = cameraGO.AddComponent<HDAdditionalCameraData>();
            if (cameraData == null)
            {
                throw new MissingComponentException("Missing HDAdditionalCameraData component");
            }
            camera.cullingMask = 1 << interactableLayer;

            #region HDRP camera settings
            cameraData.backgroundColorHDR = backgroundColor;
            cameraData.clearColorMode = HDAdditionalCameraData.ClearColorMode.Color;
            cameraData.volumeLayerMask = 0; // ensure no volume effects will affect the object id color
            cameraData.probeLayerMask = 0; // ensure no probe effects will affect the object id color
            #endregion

            // Create tempory hotspot object
            List<GameObject> tempObjects = new List<GameObject>();
            foreach (var hotspot in _target.Hotspots)
            {
                GameObject tempGO = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                tempGO.hideFlags = HideFlags.HideAndDontSave;
                tempGO.transform.SetParent(hotspot.Hitbox.transform);
                tempGO.transform.localScale = new Vector3(0.5f, 0.01f, 0.5f);
                tempGO.transform.localPosition = Vector3.zero;
                tempGO.layer = interactableLayer;
                tempObjects.Add(tempGO);
            }

            int size = Mathf.Min(Mathf.NextPowerOfTwo((int)_target.Quality), 8192);
            Cubemap cubemap = new Cubemap(size * 6, TextureFormat.ARGB32, false);
            Texture2D texture = new Texture2D(cubemap.width * (cubemapFormat == CubemapFormat.Single ? 6 : 1), cubemap.height, TextureFormat.ARGB32, false);

            try
            {
                InitializeShader();
                if (objectIdConverter == null)
                {
                    objectIdConverter = new Material(Shader.Find("Hidden/ObjectId"));
                    colorId = Shader.PropertyToID("_IdColor");
                }

                // Set Object Color
                for (int i = 0; i < objects.Length; i++)
                {
                    TreasuredObject obj = objects[i];
                    Renderer[] renderers = objects[i].GetComponentsInChildren<Renderer>();
                    MaterialPropertyBlock mpb = new MaterialPropertyBlock();
                    foreach (var renderer in renderers)
                    {
                        defaultLayers[renderer] = renderer.gameObject.layer;
                        renderer.gameObject.layer = interactableLayer;
                        defaultMaterials[renderer] = renderer.sharedMaterial;
                        renderer.sharedMaterial = objectIdConverter;
                        renderer.GetPropertyBlock(mpb);
                        renderer.shadowCastingMode = ShadowCastingMode.Off;
                        renderer.lightProbeUsage = LightProbeUsage.Off;
                        mpb.SetColor("_IdColor", maskColor);
                        renderer.SetPropertyBlock(mpb);
                    }
                }

                var hotspots = _target.Hotspots;
                if (hotspots == null || hotspots.Length == 0)
                {
                    throw new InvalidOperationException("No active hotspots.");
                }
                ValidateOutputDirectory();

                string extension = _target.Format.ToString().ToLower();


                int count = hotspots.Length;

                for (int index = 0; index < count; index++)
                {
                    Hotspot current = hotspots[index];
                    string progressTitle = $"Exporting Mask ({index + 1}/{count})";
                    string progressText = $"Generating data for {current.name}...";
                    EditorUtility.DisplayProgressBar(progressTitle, progressText, 0.33f);

                    camera.transform.SetPositionAndRotation(current.Camera.transform.position, Quaternion.identity);

                    if (!camera.RenderToCubemap(cubemap))
                    {
                        throw new System.NotSupportedException("Current graphic device/platform does not support RenderToCubemap.");
                    }
                    var di = CreateDirectory(DefaultOutputFolderPath, _target.OutputFolderName, "images", current.Id);
                    for (int i = 0; i < 6; i++)
                    {
                        EditorUtility.DisplayProgressBar(progressTitle, progressText, 0.33f + i * 0.11f);
                        texture.SetPixels(i * size, 0, size, size, cubemap.GetPixels((CubemapFace)i));
                    }
                    FlipPixels(texture, true, true);
                    ImageEncoder.Encode(texture, di.FullName, "mask", _target.Format, compression);
                }
            }
            finally
            {
                GameObject.DestroyImmediate(cubemap);
                GameObject.DestroyImmediate(texture);
                // Restore materials
                foreach (var kvp in defaultMaterials)
                {
                    kvp.Key.sharedMaterial = kvp.Value;
                }
                // Restore layer
                foreach (var kvp in defaultLayers)
                {
                    kvp.Key.gameObject.layer = kvp.Value;
                }

                foreach (var tempObject in tempObjects)
                {
                    GameObject.DestroyImmediate(tempObject);
                }

                #region Restore settings
                RenderTexture.active = activeRT;
                #endregion

                if (cameraGO != null)
                {
                    GameObject.DestroyImmediate(cameraGO);
                    cameraGO = null;
                }
            }
        }

        string SimplifyCubemapFace(CubemapFace cubemapFace)
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

        public void FlipPixels(Texture2D texture, bool flipX, bool flipY)
        {
            Color32[] originalPixels = texture.GetPixels32();

            var flippedPixels = Enumerable.Range(0, texture.width * texture.height).Select(index =>
            {
                int x = index % texture.width;
                int y = index / texture.width;
                if (flipX)
                    x = texture.width - 1 - x;

                if (flipY)
                    y = texture.height - 1 - y;

                return originalPixels[y * texture.width + x];
            }
            );

            texture.SetPixels32(flippedPixels.ToArray());
            texture.Apply();
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
                if (options.HasFlag(ExportOptions.JSON))
                {
                    ExportJson();
                }
                if (options.HasFlag(ExportOptions.PanoramicImages))
                {
                    ExportPanoramicImages();
                }
                if (options.HasFlag(ExportOptions.Mask))
                {
                    ExportMask();
                }
            }
            catch (TargetNotAssignedException e)
            {
                Debug.LogError(e.Message, e.Object);
            }
            catch (MissingFieldException e)
            {
                Debug.LogError(e.Message);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                Dispose();
            }
        }

        internal void OnGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUI.BeginChangeCheck();
                string newOutputFolderName = EditorGUILayout.TextField(new GUIContent("Output Folder Name"), _outputFolderName.stringValue);
                if (EditorGUI.EndChangeCheck() && !string.IsNullOrWhiteSpace(newOutputFolderName))
                {
                    _outputFolderName.stringValue = newOutputFolderName;
                }
                if (GUILayout.Button(Styles.folderOpened, EditorStyles.label, GUILayout.Width(20), GUILayout.Height(18)))
                {
                    Application.OpenURL(TreasuredMapExporter.DefaultOutputFolderPath);
                }
            }
            EditorGUILayout.PropertyField(_serializedObject.FindProperty("_format"));
            EditorGUILayout.PropertyField(_serializedObject.FindProperty("_quality"));
            using (new EditorGUILayout.HorizontalScope())
            {
                compression = EditorGUILayout.IntSlider("Compression", compression, 0, 100);
                EditorGUILayout.LabelField("%", GUILayout.Width(20));
            }
            cubemapFormat = (CubemapFormat)EditorGUILayout.EnumPopup("Cubemap Format", cubemapFormat);
            OnExportOptionsGUI();
            if (GUILayout.Button(new GUIContent("Export"), GUILayout.Height(24)))
            {
                Export(exportOptions);
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

        private Camera ValidateCamera()
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                throw new Exception("Camera not found. Please make sure there is active camera in the scene.");
            }
            return camera;
        }

        private DirectoryInfo CreateDirectory(params string[] paths)
        {
            return Directory.CreateDirectory(Path.Combine(paths));
        }

        private void InitializeShader()
        {
            if (_equirectangularConverter == null)
            {
                _equirectangularConverter = new Material(Shader.Find("Hidden/I360CubemapToEquirectangular"));
                _paddingXId = Shader.PropertyToID("_PaddingX");
            }
            
        }

        public void Dispose()
        {
            if (_cubemapRT != null)
            {
                RenderTexture.ReleaseTemporary(_cubemapRT);
                _cubemapRT = null;
            }

            if (_equirectangularRT != null)
            {
                RenderTexture.ReleaseTemporary(_equirectangularRT);
                _equirectangularRT = null;
            }

            if (_outputTexture != null)
            {
                GameObject.DestroyImmediate(_outputTexture);
                _outputTexture = null;
            }

            if (_equirectangularConverter != null)
            {
                GameObject.DestroyImmediate(_equirectangularConverter);
                _equirectangularConverter = null;
            }

            if (objectIdConverter != null)
            {
                GameObject.DestroyImmediate(objectIdConverter);
                objectIdConverter = null;
            }
        }
    }
}
