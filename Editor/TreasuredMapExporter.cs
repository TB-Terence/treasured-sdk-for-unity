using Newtonsoft.Json;
using Simple360Render;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Treasured.UnitySdk.Editor
{
    internal partial class TreasuredMapEditor
    {
        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ContractResolver = CustomContractResolver.Instance
        };

        private string _sceneName;

        private string GetAbosluteOutputDirectory(string folderName)
        {
            return Path.Combine(Utility.ProjectPath, _outputDirectory.stringValue, folderName);
        }

        private void ExportAll(string directory)
        {
            ExportPanoramicImages(directory);
            ExportJson(directory);
        }

        private void ExportPanoramicImages(string directory)
        {
            if (string.IsNullOrEmpty(directory))
            {
                return;
            }
            Capture(Target, Camera.main, directory);
            //Capture2(Target, Camera.main, directory);
        }

        private void Capture2(TreasuredMap map, Camera camera, string directory)
        {
            int width = (int)Target.Data.Quality;
            RenderTexture equirect = RenderTexture.GetTemporary(width, width / 2, 0);
            RenderTexture cubemapLeftEye = RenderTexture.GetTemporary(width / 2, width / 2, 0);
            cubemapLeftEye.dimension = TextureDimension.Cube;
            RenderTexture cubemapRightEye = RenderTexture.GetTemporary(width / 2, width / 2, 0);
            cubemapRightEye.dimension = TextureDimension.Cube;
            camera.stereoSeparation = 0.064f; // Eye separation (IPD) of 64mm.
            camera.RenderToCubemap(cubemapLeftEye, 63, Camera.MonoOrStereoscopicEye.Left);

            camera.RenderToCubemap(cubemapRightEye, 63, Camera.MonoOrStereoscopicEye.Right);

            cubemapLeftEye.ConvertToEquirect(equirect, Camera.MonoOrStereoscopicEye.Left);
            cubemapRightEye.ConvertToEquirect(equirect, Camera.MonoOrStereoscopicEye.Right);
            RenderTexture.ReleaseTemporary(equirect);
            RenderTexture.ReleaseTemporary(cubemapLeftEye);
            RenderTexture.ReleaseTemporary(cubemapRightEye);
        }

        private void ExportJson(string directory)
        {
            if (string.IsNullOrEmpty(directory))
            {
                return;
            }
            Target.Data.GenerateHotspots(_hotspots);
            Target.Data.GenerateInteractables(_interactables);
            Target.Data.Validate();
            List<TreasuredObject> objects = new List<TreasuredObject>();
            objects.AddRange(_hotspots);
            objects.AddRange(_interactables);
            foreach (var hotspot in _hotspots)
            {
                if (hotspot.Data is HotspotData data)
                {
                    data.FindVisibleTargets(objects, _interactableLayer.intValue);
                }
            }
            // TODO: Add Select Object Id Validation
            string json = JsonConvert.SerializeObject(Target.Data, Formatting.Indented, JsonSettings);
            File.WriteAllText(Path.Combine(directory, $"data.json"), json);
        }

        private static Material _equirectangularConverter;
        private static int _paddingX;

        private void Capture(TreasuredMap map, Camera camera, string directory)
        {
            // TODO: Find a better solution for seam in output images
            Volume[] volumes = GameObject.FindObjectsOfType<Volume>();
            Volume globalVolume = null;
            Exposure exposure = null;
            ExposureMode previousMode = ExposureMode.Fixed;
            float previousFixedExposure = 13;
            if (volumes.Length > 0)
            {
                globalVolume = volumes.FirstOrDefault(x => x.isGlobal);
                if (globalVolume)
                {
                    if(globalVolume.profile.TryGet<Exposure>(out exposure))
                    {
                        previousMode = exposure.mode.value;
                        previousFixedExposure = exposure.fixedExposure.value;
                        exposure.mode.value = ExposureMode.Fixed;
                        exposure.fixedExposure.value = _fixedExposure.floatValue;
                    }
                }
            }

            if (camera == null)
            {
                camera = Camera.main;
                if (camera == null)
                {
                    Debug.LogError("No active camera found in scene.");
                    return;
                }
            }
            #region Get current camera/RenderTexture settings
            RenderTexture camTarget = camera.targetTexture;
            Vector3 originalCameraPos = camera.transform.position;
            Quaternion originalCameraRot = camera.transform.rotation;
            RenderTexture activeRT = RenderTexture.active;
            #endregion

            #region Create output directories
            string qualityFolderName = $"{Enum.GetName(typeof(ImageQuality), map.Data.Quality).ToLower()}/";
            string qualityFolderDirectory = Path.Combine(directory, qualityFolderName);
            if (Directory.Exists(qualityFolderDirectory))
            {
                DirectoryInfo info = new DirectoryInfo(qualityFolderDirectory);
                FileInfo[] fileInfos = info.GetFiles();
                if (info.GetFiles().Length > 0)
                {
                    IEnumerable<string> files = fileInfos.Select(x => Path.GetFileNameWithoutExtension(x.Name)).Except(_hotspots.Select(x => x.Data.Id));
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
            bool encodeAsJPEG = map.Data.Format == SDK.ImageFormat.JPG;
            int cubeMapSize = Mathf.Min(Mathf.NextPowerOfTwo((int)map.Data.Quality), 8192);
            bool faceCameraDirection = true;
            #endregion

            #region Create RenderTexture/Texture2D
            RenderTexture cubeMapTexture = RenderTexture.GetTemporary(cubeMapSize, cubeMapSize, 0);
            cubeMapTexture.dimension = TextureDimension.Cube;
            cubeMapTexture.useMipMap = false; // RenderTexture.GetTemporary(cubeMapSize, cubeMapSize, 0);
            RenderTexture equirectangularTexture = RenderTexture.GetTemporary(cubeMapSize, cubeMapSize / 2, 0);
            equirectangularTexture.dimension = TextureDimension.Tex2D;
            equirectangularTexture.useMipMap = false;
            Texture2D outputTexture = new Texture2D(equirectangularTexture.width, equirectangularTexture.height, TextureFormat.RGB24, false);
            #endregion

            if (_equirectangularConverter == null)
            {
                _equirectangularConverter = new Material(Shader.Find("Hidden/I360CubemapToEquirectangular"));
                _paddingX = Shader.PropertyToID("_PaddingX");
            }

            try
            {
                var exportables = _hotspots.Where(hotspot => hotspot.gameObject.activeSelf).ToArray();
                for (int index = 0; index < exportables.Length; index++)
                {
                    Hotspot hotspot = exportables[index];

                    var fileName = $"{hotspot.Data.Id}.{map.Data.Format.ToString().ToLower()}";
                    // Move the camera in the right position
                    camera.transform.SetPositionAndRotation(hotspot.transform.position, Quaternion.identity);

                    EditorUtility.DisplayProgressBar($"Exporting image ({index + 1}/{exportables.Length})", $"Working on cubemap for {hotspot.Data.Name}", 0.33f);
                    if (!camera.RenderToCubemap(cubeMapTexture, 63))
                    {
                        throw new NotSupportedException("Rendering to cubemap is not supported on device/platform!");
                    }

                    EditorUtility.DisplayProgressBar($"Exporting image ({index + 1}/{exportables.Length})", "Applying shader...", 0.66f);
                    _equirectangularConverter.SetFloat(_paddingX, faceCameraDirection ? (camera.transform.eulerAngles.y / 360f) : 0f);
                    Graphics.Blit(cubeMapTexture, equirectangularTexture, _equirectangularConverter);

                    RenderTexture.active = equirectangularTexture;
                    outputTexture.ReadPixels(new Rect(0, 0, equirectangularTexture.width, equirectangularTexture.height), 0, 0, false);

                    //EditorUtility.DisplayProgressBar($"Exporting image ({index + 1}/{count})", $"Inserting XMP Data for {fileName}...", 0.99f);
                    byte[] bytes = encodeAsJPEG ? I360Render.InsertXMPIntoTexture2D_JPEG(outputTexture) : I360Render.InsertXMPIntoTexture2D_PNG(outputTexture);
                    if (bytes != null)
                    {
                        EditorUtility.DisplayProgressBar($"Exporting {fileName} ({index + 1}/{exportables.Length})", $"Saving {fileName}...", 0.99f);
                        string path = Path.Combine(qualityFolderDirectory, fileName);
                        File.WriteAllBytes(path, bytes);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                #region Restore settings
                camera.transform.position = originalCameraPos;
                camera.transform.rotation = originalCameraRot;
                camera.targetTexture = camTarget;
                RenderTexture.active = activeRT;

                // TODO: Find a better solution for seam in output images
                if (exposure)
                {
                    exposure.mode.value = previousMode;
                    exposure.fixedExposure.value = previousFixedExposure;
                }
                #endregion

                #region Free resources
                if (cubeMapTexture != null)
                {
                    //RenderTexture.ReleaseTemporary(cubeMapTexture);
                    cubeMapTexture.Release();
                    cubeMapTexture = null;
                }

                if (equirectangularTexture != null)
                {
                    //RenderTexture.ReleaseTemporary(equirectangularTexture);
                    equirectangularTexture.Release();
                    equirectangularTexture = null;
                }

                if (outputTexture != null)
                {
                    GameObject.DestroyImmediate(outputTexture);
                    outputTexture = null;
                }

                if (_equirectangularConverter != null)
                {
                    GameObject.DestroyImmediate(_equirectangularConverter);
                    _equirectangularConverter = null;
                }
                #endregion
            }
        }
    }
}
