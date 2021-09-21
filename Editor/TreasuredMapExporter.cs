using Newtonsoft.Json;
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Treasured.UnitySdk
{
    internal class TreasuredMapExporter
    {
        /// <summary>
        /// Default output folder in project root
        /// </summary>
        public const string DefaultOutputFolder = "Treasured Data/";
        public static readonly string DefaultOutputFolderPath = $"{Directory.GetCurrentDirectory()}/{DefaultOutputFolder}";

        #region Json
        public static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ContractResolver = ContractResolver.Instance,
            CheckAdditionalContent = true
        };
        #endregion

        #region Image
        private static Material equirectangularConverter;
        private static int paddingXId;

        private static Material objectIdConverter;
        private static int colorId;
        #endregion

        public TreasuredMap target;
        public SerializedObject serializedObject;

        public bool showInExplorer;

        private DirectoryInfo outputDirectory;

        private SerializedProperty outputFolderName;

        public TreasuredMapExporter(SerializedObject serializedObject, TreasuredMap map, bool showInExplorer = true)
        {
            this.target = map;
            this.serializedObject = serializedObject;
            this.showInExplorer = showInExplorer;
            outputFolderName = serializedObject.FindProperty("_outputFolderName");
            if (string.IsNullOrEmpty(outputFolderName.stringValue))
            {
                outputFolderName.stringValue = EditorSceneManager.GetActiveScene().name;
                serializedObject.ApplyModifiedProperties();
            }
        }

        public void OnGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            string newOutputFolderName = EditorGUILayout.TextField(new GUIContent("Output Folder Name"), outputFolderName.stringValue);
            if (EditorGUI.EndChangeCheck() && !string.IsNullOrEmpty(newOutputFolderName))
            {
                outputFolderName.stringValue = newOutputFolderName;
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("format"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("quality"));
            serializedObject.ApplyModifiedProperties();
            if (GUILayout.Button("Export Json"))
            {
                ExportJson();
                ShowInExplorer();
            }
            if (GUILayout.Button("Export Images"))
            {

            }
            if (GUILayout.Button("Export Object Ids"))
            {

            }
            if (GUILayout.Button("Export Map"))
            {

            }
            if (GUILayout.Button("Upload"))
            {
                Application.OpenURL("https://dev.world.treasured.ca/upload");
            }
        }

        private void Validate()
        {
            if (target == null)
            {
                throw new NullReferenceException("Map is not assigned for exporter.");
            }
            try
            {
                outputDirectory = Directory.CreateDirectory($"{DefaultOutputFolderPath}/{target.OutputFolderName}");
            }
            catch (ArgumentException)
            {
                throw new ArgumentException($"Invalid folder name : {target.OutputFolderName}");
            }
        }

        private void ShowInExplorer()
        {
            if (showInExplorer)
            {
                Application.OpenURL(outputDirectory.FullName);
            }
        }

        private void ExportJson()
        {
            Validate();
            string jsonPath = Path.Combine(outputDirectory.FullName, "data.json");
            string json = JsonConvert.SerializeObject(target, Formatting.Indented, JsonSettings);
            File.WriteAllText(jsonPath, json);
        }


        //public static void Export(TreasuredMap map, string folderName)
        //{
        //    try
        //    {
        //        _map = map ?? throw new ArgumentException("Map is not assigned.");
        //        _outputDirectory = outputDirectory ?? throw new ArgumentException("Directory is not assigned.");
        //        ExportJson(map, outputDirectory);
        //        ExportHotspotImages(map, outputDirectory);
        //        Application.OpenURL(outputDirectory.FullName);
        //        //EditorApplication.playModeStateChanged -= OnEnterPlayMode;
        //        //EditorApplication.playModeStateChanged += OnEnterPlayMode;
        //        //EditorApplication.EnterPlaymode();
        //    }
        //    catch(Exception e)
        //    {
        //        Debug.LogError(e.Message);
        //    }
        //}

        //private static void StartExport()
        //{
        //    try
        //    {
        //        ExportJson(_map, _outputDirectory);
        //        ExportHotspotImages(_map, _outputDirectory);
        //    }
        //    catch(Exception e)
        //    {
        //        throw e;
        //    }
        //    finally
        //    {
        //        EditorApplication.ExitPlaymode();
        //        EditorApplication.playModeStateChanged -= OnEnterPlayMode;
        //        Application.OpenURL(DefaultOutputFolderPath);
        //    }
        //}

        //private static void OnEnterPlayMode(PlayModeStateChange mode)
        //{
        //    if (mode == PlayModeStateChange.EnteredPlayMode)
        //    {
        //        StartExport();
        //    }
        //}

        //public static void ExportJson(TreasuredMap map, DirectoryInfo outputDirectory)
        //{
        //    if (!outputDirectory.Exists)
        //    {
        //        throw new ArgumentException("Unable to export json. The given directory does not exist.");
        //    }
        //    try
        //    {

        //    }
        //    catch(JsonException e)
        //    {
        //        throw e;
        //    }
        //}

        //public static void ExportHotspotImages(TreasuredMap map, DirectoryInfo outputDirectory)
        //{
        //    if (!outputDirectory.Exists)
        //    {
        //        throw new ArgumentException("Unable to export hotspot images. The given directory does not exist.");
        //    }
        //    if (!map || map.Hotspots == null)
        //    {
        //        throw new NullReferenceException();
        //    }
        //    Camera camera = Camera.main;
        //    if(camera == null)
        //    {
        //        throw new Exception("Camera not found. Please make sure there is active camera in the scene.");
        //    }

        //    #region Get current camera/RenderTexture settings
        //    RenderTexture camTarget = camera.targetTexture;
        //    Vector3 originalCameraPos = camera.transform.position;
        //    Quaternion originalCameraRot = camera.transform.rotation;
        //    RenderTexture activeRT = RenderTexture.active;
        //    #endregion

        //    if (map.Hotspots.Count == 0)
        //    {
        //        Debug.LogError("Map does not contains Hotspot");
        //        return;
        //    }

        //    // Create image directory
        //    DirectoryInfo imageDirectory = Directory.CreateDirectory(Path.Combine(outputDirectory.FullName, map.quality.ToString().ToLower()));

        //    int cubeMapSize = Mathf.Min(Mathf.NextPowerOfTwo((int)map.quality), 8192);
        //    string format = map.format.ToString().ToLower();
        //    int count = map.Hotspots.Count;

        //    // Create textures
        //    RenderTexture cubeMapTexture = RenderTexture.GetTemporary(cubeMapSize, cubeMapSize, 0);
        //    cubeMapTexture.dimension = TextureDimension.Cube;
        //    cubeMapTexture.useMipMap = false;
        //    RenderTexture equirectangularTexture = RenderTexture.GetTemporary(cubeMapSize, cubeMapSize / 2, 0);
        //    equirectangularTexture.dimension = TextureDimension.Tex2D;
        //    equirectangularTexture.useMipMap = false;
        //    Texture2D outputTexture = new Texture2D(equirectangularTexture.width, equirectangularTexture.height, TextureFormat.RGB24, false);
        //    Texture2D Texture = new Texture2D(equirectangularTexture.width, equirectangularTexture.height, TextureFormat.RGB24, false);

        //    SetShaderConfig();

        //    // Convert object ids from color string -> id to id -> Color
        //    Dictionary<string, Color> objectIds = map.ObjectIds.ToDictionary(x => x.Value, (x) =>
        //    {
        //        ColorUtility.TryParseHtmlString(x.Key, out Color color);
        //        return color;
        //    }); 

        //    try
        //    {
        //        for (int index = 0; index < count; index++)
        //        {
        //            Hotspot hotspot = map.Hotspots[index];

        //            var fileName = $"{hotspot.Id}.{format}";
        //            // Move the camera in the right position
        //            camera.transform.SetPositionAndRotation(hotspot.cameraTransform.position, Quaternion.identity);

        //            EditorUtility.DisplayProgressBar($"Exporting ({index + 1}/{count})", $"Generating cubemap for {hotspot.name}", 0.33f);
        //            if (!camera.RenderToCubemap(cubeMapTexture, 63))
        //            {
        //                throw new NotSupportedException("Rendering to cubemap is not supported on device/platform!");
        //            }

        //            EditorUtility.DisplayProgressBar($"Exporting image ({index + 1}/{count})", "Converting...", 0.66f);

        //            equirectangularConverter.SetFloat(paddingXId, camera.transform.eulerAngles.y / 360f);
        //            Graphics.Blit(cubeMapTexture, equirectangularTexture, equirectangularConverter);

        //            RenderTexture.active = equirectangularTexture;
        //            outputTexture.ReadPixels(new Rect(0, 0, equirectangularTexture.width, equirectangularTexture.height), 0, 0, false);

        //            byte[] bytes = map.format == Format.JPG ? outputTexture.EncodeToJPG() : outputTexture.EncodeToPNG();
        //            if (bytes != null)
        //            {
        //                EditorUtility.DisplayProgressBar($"Exporting ({index + 1}/{count})", $"Generating image file...", 0.99f);
        //                string path = Path.Combine(imageDirectory.FullName, $"{hotspot.Id}.{format}");
        //                File.WriteAllBytes(path, bytes);
        //            }
        //        }
        //    }
        //    catch(Exception e)
        //    {
        //        throw e;
        //    }
        //    finally
        //    {
        //        EditorUtility.ClearProgressBar();
        //        #region Restore settings
        //        camera.transform.position = originalCameraPos;
        //        camera.transform.rotation = originalCameraRot;
        //        camera.targetTexture = camTarget;
        //        RenderTexture.active = activeRT;
        //        #endregion

        //        #region Free resources
        //        if (cubeMapTexture != null)
        //        {
        //            //RenderTexture.ReleaseTemporary(cubeMapTexture);
        //            cubeMapTexture.Release();
        //            cubeMapTexture = null;
        //        }

        //        if (equirectangularTexture != null)
        //        {
        //            //RenderTexture.ReleaseTemporary(equirectangularTexture);
        //            equirectangularTexture.Release();
        //            equirectangularTexture = null;
        //        }

        //        if (outputTexture != null)
        //        {
        //            GameObject.DestroyImmediate(outputTexture);
        //            outputTexture = null;
        //        }

        //        if (equirectangularConverter != null)
        //        {
        //            GameObject.DestroyImmediate(equirectangularConverter);
        //            equirectangularConverter = null;
        //        }
        //        #endregion
        //    }
        //}

        //private static void SetShaderConfig()
        //{
        //    if (equirectangularConverter == null)
        //    {
        //        equirectangularConverter = new Material(Shader.Find("Hidden/I360CubemapToEquirectangular"));
        //        paddingXId = Shader.PropertyToID("_PaddingX");
        //    }
        //    //if (objectIdConverter == null)
        //    //{
        //    //    objectIdConverter = new Material(Shader.Find("Hidden/ObjectIdRenderer"));
        //    //    colorId = Shader.PropertyToID("_IdColor");
        //    //}
        //}
    }
}
