using Newtonsoft.Json;
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace Treasured.UnitySdk
{
    [Flags]
    public enum ExportOptions
    {
        JSON = 1 << 0,
        PanoramicImages = 1 << 1,
        //ObjectIds = 1 << 2,
        All = JSON | PanoramicImages //| ObjectIds
    }

    internal class TreasuredMapExporter : IDisposable
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

        private static Material objectIdConverter;
        private static int colorId;
        #endregion

        public TreasuredMap target;
        public SerializedObject serializedObject;

        public bool showInExplorer;

        private DirectoryInfo outputDirectory;


        private int paddingXId;

        private RenderTexture cubemapRT;
        private RenderTexture equirectangularRT;
        private Texture2D outputTexture;
        private Material equirectangularConverter;


        public TreasuredMapExporter(SerializedObject serializedObject, TreasuredMap map, bool showInExplorer = true)
        {
            this.target = map;
            this.serializedObject = serializedObject;
            this.showInExplorer = showInExplorer;
        }

        private void ValidateOutputDirectory()
        {
            if (target == null)
            {
                throw new NullReferenceException("Map is not assigned for exporter.");
            }
            try
            {
                outputDirectory = Directory.CreateDirectory($"{DefaultOutputFolderPath}/{target.OutputFolderName}");
            }
            catch (Exception ex) when (ex is IOException || ex is ArgumentException)
            {
                throw new ArgumentException($"Invalid folder name : {target.OutputFolderName}");
            }
            catch
            {
                throw;
            }
        }

        private void ExportJson()
        {
            JsonValidator.ValidateMap(target);
            ValidateOutputDirectory();
            foreach (var hotspot in target.Hotspots)
            {
                hotspot.ComputeVisibleTargets();
            }
            string jsonPath = Path.Combine(outputDirectory.FullName, "data.json");
            string json = JsonConvert.SerializeObject(target, Formatting.Indented, JsonSettings);
            File.WriteAllText(jsonPath, json);
        }

        private void ExportPanoramicImages()
        {
            var hotspots = target.Hotspots;
            if (hotspots == null || hotspots.Length == 0)
            {
                throw new InvalidOperationException("No active hotspots.");
            }
            ValidateOutputDirectory();
            var camera = ValidateCamera();

            #region Get camera settings
            RenderTexture camTarget = camera.targetTexture;
            Vector3 originalCameraPos = camera.transform.position;
            Quaternion originalCameraRot = camera.transform.rotation;
            RenderTexture activeRT = RenderTexture.active;
            #endregion

            string quality = target.Quality.ToString().ToLower();
            string extension = target.Format.ToString().ToLower();

            int cubeMapSize = Mathf.Min(Mathf.NextPowerOfTwo((int)target.Quality), 8192);

            int count = hotspots.Length;

            InitializeShader();

            // Create textures
            cubemapRT = RenderTexture.GetTemporary(cubeMapSize, cubeMapSize, 0);
            cubemapRT.dimension = TextureDimension.Cube;
            cubemapRT.useMipMap = false;
            equirectangularRT = RenderTexture.GetTemporary(cubeMapSize, cubeMapSize / 2, 0);
            equirectangularRT.dimension = TextureDimension.Tex2D;
            equirectangularRT.useMipMap = false;
            outputTexture = new Texture2D(equirectangularRT.width, equirectangularRT.height, TextureFormat.RGB24, false);

            try
            {
                for (int index = 0; index < count; index++)
                {
                    Hotspot current = hotspots[index];
                    string progressTitle = $"Exporting ({index + 1}/{count})";
                    string progressText = $"Generating data for {current.name}...";
                    EditorUtility.DisplayProgressBar(progressTitle, progressText, 0.33f);

                    // Move the camera in the right position
                    camera.transform.SetPositionAndRotation(current.transform.position + current.CameraPositionOffset, Quaternion.identity);

                    if (!camera.RenderToCubemap(cubemapRT, 63))
                    {
                        throw new NotSupportedException("Rendering to cubemap is not supported on device/platform!");
                    }

                    equirectangularConverter.SetFloat(paddingXId, camera.transform.eulerAngles.y / 360f);
                    EditorUtility.DisplayProgressBar(progressTitle, progressText, 0.66f);
                    Graphics.Blit(cubemapRT, equirectangularRT, equirectangularConverter);

                    RenderTexture.active = equirectangularRT;
                    outputTexture.ReadPixels(new Rect(0, 0, equirectangularRT.width, equirectangularRT.height), 0, 0, false);

                    byte[] bytes = target.Format == ImageFormat.JPG ? outputTexture.EncodeToJPG() : outputTexture.EncodeToPNG();
                    if (bytes != null)
                    {
                        EditorUtility.DisplayProgressBar(progressTitle, progressText, 0.99f);
                        var directory = CreateDirectory(DefaultOutputFolderPath, target.OutputFolderName, "images", current.Id);
                        string imagePath = Path.Combine(directory.FullName, $"{quality}.{extension}");
                        File.WriteAllBytes(imagePath, bytes);
                    }
                }
            }
            catch(Exception e)
            {
                throw e;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                #region Restore settings
                camera.transform.position = originalCameraPos;
                camera.transform.rotation = originalCameraRot;
                camera.targetTexture = camTarget;
                RenderTexture.active = activeRT;
                #endregion

                Dispose();
            }
        }

        private void ExportObjectIds()
        {
            //    // Convert object ids from color string -> id to id -> Color
            //    Dictionary<string, Color> objectIds = map.ObjectIds.ToDictionary(x => x.Value, (x) =>
            //    {
            //        ColorUtility.TryParseHtmlString(x.Key, out Color color);
            //        return color;
            //    });
        }

        public void Export(ExportOptions options)
        {
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
                //if (options.HasFlag(ExportOptions.ObjectIds))
                //{
                //    ExportObjectIds();
                //}
                if (showInExplorer && outputDirectory != null)
                {
                    Application.OpenURL(outputDirectory.FullName);
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
            if (equirectangularConverter == null)
            {
                equirectangularConverter = new Material(Shader.Find("Hidden/I360CubemapToEquirectangular"));
                paddingXId = Shader.PropertyToID("_PaddingX");
            }
            //if (objectIdConverter == null)
            //{
            //    objectIdConverter = new Material(Shader.Find("Hidden/ObjectIdRenderer"));
            //    colorId = Shader.PropertyToID("_IdColor");
            //}
        }

        public void Dispose()
        {
            if (cubemapRT != null)
            {
                RenderTexture.ReleaseTemporary(cubemapRT);
                cubemapRT = null;
            }

            if (equirectangularRT != null)
            {
                RenderTexture.ReleaseTemporary(equirectangularRT);
                equirectangularRT = null;
            }

            if (outputTexture != null)
            {
                GameObject.DestroyImmediate(outputTexture);
                outputTexture = null;
            }

            if (equirectangularConverter != null)
            {
                GameObject.DestroyImmediate(equirectangularConverter);
                equirectangularConverter = null;
            }
        }
    }
}
