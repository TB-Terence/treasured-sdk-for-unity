using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
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
        ObjectIds = 1 << 2,
        All = JSON | PanoramicImages | ObjectIds
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

        private DirectoryInfo outputDirectory;


        private int paddingXId;

        private RenderTexture cubemapRT;
        private RenderTexture equirectangularRT;
        private Texture2D outputTexture;
        private Material equirectangularConverter;


        public TreasuredMapExporter(SerializedObject serializedObject, TreasuredMap map)
        {
            this.target = map;
            this.serializedObject = serializedObject;
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
            var camera = ValidateCamera(); // use default camera settings to render 360 images

            #region Get camera settings
            RenderTexture camTarget = camera.targetTexture;
            Vector3 originalCameraPos = camera.transform.position;
            Quaternion originalCameraRot = camera.transform.rotation;
            RenderTexture activeRT = RenderTexture.active;
            #endregion

            string fileName = target.Quality.ToString().ToLower();

            string extension = target.Format.ToString().ToLower();

            int cubeMapSize = Mathf.Min(Mathf.NextPowerOfTwo((int)target.Quality), 8192);

            int count = hotspots.Length;

            InitializeShader();

            // Create textures
            if (cubemapRT == null)
            {
                cubemapRT = RenderTexture.GetTemporary(cubeMapSize, cubeMapSize, 0);
                cubemapRT.dimension = TextureDimension.Cube;
                cubemapRT.useMipMap = false;
            }
            if (equirectangularRT == null)
            {
                equirectangularRT = RenderTexture.GetTemporary(cubeMapSize, cubeMapSize / 2, 0);
                equirectangularRT.dimension = TextureDimension.Tex2D;
                equirectangularRT.useMipMap = false;
            }
            outputTexture = new Texture2D(equirectangularRT.width, equirectangularRT.height, TextureFormat.RGB24, false);

            try
            {
                for (int index = 0; index < count; index++)
                {
                    Hotspot current = hotspots[index];
                    string progressTitle = $"Exporting Panoramic Image ({index + 1}/{count})";
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
                        string imagePath = Path.Combine(directory.FullName, $"{fileName}.{extension}");

                        if (target.Format == ImageFormat.WEBP)
                        {
                            ImageEncoder.EncodeToWEBP(bytes, imagePath, 100);
                        }
                        else
                        {
                            File.WriteAllBytes(imagePath, bytes);
                        }
                    }
                }
            }
            catch(Exception e)
            {
                throw e;
            }
            finally
            {
                #region Restore settings
                camera.transform.position = originalCameraPos;
                camera.transform.rotation = originalCameraRot;
                camera.targetTexture = camTarget;
                RenderTexture.active = activeRT;
                #endregion
            }
        }

        private void ExportObjectIds()
        {
            ValidateOutputDirectory();

            TreasuredObject[] objects = target.GetComponentsInChildren<TreasuredObject>();
            Color[] objectIds = objects.Select(x => x.ObjectId).ToArray();
            Dictionary<Renderer, Material> defaultMaterials = new Dictionary<Renderer, Material>();
            Dictionary<Renderer, int> defaultLayers = new Dictionary<Renderer, int>(); // doesn't seem necessary but it will prevent breaking existing objects.
            int interactableLayer = serializedObject.FindProperty("_interactableLayer").intValue; // single layer

            RenderTexture activeRT = RenderTexture.active;

            var cameraGO = new GameObject("Panoramic Image Camera"); // creates a temporary camera with some default settings.
            cameraGO.hideFlags = HideFlags.HideAndDontSave;
            var camera = cameraGO.AddComponent<Camera>();
            var cameraData = cameraGO.AddComponent<HDAdditionalCameraData>();
            if (cameraData == null)
            {
                throw new MissingComponentException("Missing HDAdditionalCameraData component");
            }
            camera.cullingMask = 1 << interactableLayer;

            #region HDRP camera settings
            cameraData.backgroundColorHDR = Color.black;
            cameraData.clearColorMode = HDAdditionalCameraData.ClearColorMode.Color;
            cameraData.volumeLayerMask = 0; // ensure no volume effects will affect the object id color
            cameraData.probeLayerMask = 0; // ensure no probe effects will affect the object id color

            #endregion
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
                    Renderer[] renderers = objects[i].GetComponentsInChildren<Renderer>();
                    MaterialPropertyBlock mpb = new MaterialPropertyBlock();
                    foreach (var renderer in renderers)
                    {
                        defaultLayers[renderer] = renderer.gameObject.layer;
                        renderer.gameObject.layer = interactableLayer;
                        defaultMaterials[renderer] = renderer.sharedMaterial;
                        renderer.sharedMaterial = objectIdConverter;
                        renderer.GetPropertyBlock(mpb);
                        mpb.SetColor("_IdColor", objectIds[i]);
                        renderer.SetPropertyBlock(mpb);
                    }
                }

                var hotspots = target.Hotspots;
                if (hotspots == null || hotspots.Length == 0)
                {
                    throw new InvalidOperationException("No active hotspots.");
                }
                ValidateOutputDirectory();

                string extension = target.Format.ToString().ToLower();

                int cubeMapSize = Mathf.Min(Mathf.NextPowerOfTwo((int)target.Quality), 8192);

                int count = hotspots.Length;

                // Create textures
                if(cubemapRT == null)
                {
                    cubemapRT = RenderTexture.GetTemporary(cubeMapSize, cubeMapSize, 0);
                    cubemapRT.dimension = TextureDimension.Cube;
                    cubemapRT.useMipMap = false;
                }
                if (equirectangularRT == null)
                {
                    equirectangularRT = RenderTexture.GetTemporary(cubeMapSize, cubeMapSize / 2, 0);
                    equirectangularRT.dimension = TextureDimension.Tex2D;
                    equirectangularRT.useMipMap = false;
                }
                outputTexture = new Texture2D(equirectangularRT.width, equirectangularRT.height, TextureFormat.RGB24, false);

                for (int index = 0; index < count; index++)
                {
                    Hotspot current = hotspots[index];
                    string progressTitle = $"Exporting Object Id ({index + 1}/{count})";
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

                    byte[] bytes = outputTexture.EncodeToPNG(); // object id outputs will be fixed to PNG only due to loss in JPG
                    if (bytes != null)
                    {
                        EditorUtility.DisplayProgressBar(progressTitle, progressText, 0.99f);
                        var directory = CreateDirectory(DefaultOutputFolderPath, target.OutputFolderName, "images", current.Id);
                        string imagePath = Path.Combine(directory.FullName, $"id.png");
                        File.WriteAllBytes(imagePath, bytes);
                    }
                }
            }
            finally
            {
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
                if (options.HasFlag(ExportOptions.ObjectIds))
                {
                    ExportObjectIds();
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

            if (objectIdConverter != null)
            {
                GameObject.DestroyImmediate(objectIdConverter);
                objectIdConverter = null;
            }
        }
    }
}
