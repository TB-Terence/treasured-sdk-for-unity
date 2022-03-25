using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

namespace Treasured.UnitySdk
{
    public class CubemapExporter : Exporter
    {
        public const int MAXIMUM_CUDA_TEXTURE_WIDTH = 4096;
        public const int MAXIMUM_CUBEMAP_FACE_WIDTH = 1360; // 4096 / 3 round to nearest tenth
        public const int MAXIMUM_CUBEMAP_WIDTH = 8192;

        public ImageFormat imageFormat = ImageFormat.Ktx2;
        public ImageQuality imageQuality = ImageQuality.High;
        public CubemapFormat cubemapFormat = CubemapFormat.IndividualFace;
        [SerializeField]
        private int _cubemapSize = MAXIMUM_CUBEMAP_FACE_WIDTH;
        [HideInInspector]
        public bool flipY = true;

        [SerializeField]
        [HideInInspector]
        private int _qualityPercentage = 75;

        public override void Export()
        {
            var hotspots = ValidateHotspots(Map);
            var camera = ValidateCamera(); // use default camera settings to render 360 images

            #region Get camera settings
            RenderTexture camTarget = camera.targetTexture;
            Vector3 originalCameraPos = camera.transform.position;
            Quaternion originalCameraRot = camera.transform.rotation;
            #endregion

            //int size = 1024;//Mathf.Min(Mathf.NextPowerOfTwo((int)map.Quality), 8192);

            int count = hotspots.Length;

            Cubemap cubemap = new Cubemap(cubemapFormat == CubemapFormat.IndividualFace ? (int)imageQuality : _cubemapSize, TextureFormat.ARGB32, false);
            Texture2D texture = null;
            switch (cubemapFormat)
            {
                case CubemapFormat._3x2:
                    texture = new Texture2D(MAXIMUM_CUDA_TEXTURE_WIDTH, MAXIMUM_CUDA_TEXTURE_WIDTH, TextureFormat.ARGB32, false);
                    break;
                case CubemapFormat.IndividualFace:
                    texture = new Texture2D(cubemap.width, cubemap.height, TextureFormat.ARGB32, false);
                    break;
            };
            //  If imageFormat is KTX2 then export images as png and then later convert them to KTX2 format  
            ImageFormat imageFormatParser = imageFormat == ImageFormat.Ktx2 ? ImageFormat.PNG : imageFormat;

            try
            {
                for (int index = 0; index < count; index++)
                {
                    Hotspot current = hotspots[index];
                    string progressTitle = $"Exporting Hotspots ({index + 1}/{count})";
                    string progressText = $"Generating data for {current.name}...";

                    camera.transform.SetPositionAndRotation(current.Camera.transform.position, Quaternion.identity);

                    if (!camera.RenderToCubemap(cubemap))
                    {
                        throw new System.NotSupportedException("Current graphic device/platform does not support RenderToCubemap.");
                    }
                    var path = Directory.CreateDirectory(Path.Combine(Map.exportSettings.OutputDirectory, "images", current.Id).Replace('/', '\\'));
                    switch (cubemapFormat)
                    {
                        case CubemapFormat._3x2:
                            // FORMAT:
                            // RIGHT(+X) LEFT(-X) TOP(+Y)
                            // BOTTOM(-Y) FRONT(+Z) BACK(-Z)
                            for (int i = 0; i < 6; i++)
                            {
#if UNITY_EDITOR
                                if (UnityEditor.EditorUtility.DisplayCancelableProgressBar(progressTitle, progressText, i / 6f))
                                {
                                    throw new TreasuredException("Export canceled", "Export canceled by the user.");
                                }
#endif
                                texture.SetPixels((i % 3) * _cubemapSize, MAXIMUM_CUDA_TEXTURE_WIDTH - ((i / 3) + 1) * _cubemapSize, _cubemapSize, _cubemapSize,
                                ImageUtilies.FlipPixels(cubemap.GetPixels((CubemapFace)i), _cubemapSize, _cubemapSize, true, flipY));
                            }
                            ImageUtilies.Encode(texture, path.FullName, "cubemap", imageFormatParser, _qualityPercentage);
                            break;
                        case CubemapFormat.IndividualFace:
                            for (int i = 0; i < 6; i++)
                            {
#if UNITY_EDITOR
                                if (UnityEditor.EditorUtility.DisplayCancelableProgressBar(progressTitle, progressText, i / 6f))
                                {
                                    throw new TreasuredException("Export canceled", "Export canceled by the user.");
                                }
#endif
                                texture.SetPixels(cubemap.GetPixels((CubemapFace)i));
                                ImageUtilies.FlipPixels(texture, true, flipY);
                                ImageUtilies.Encode(texture, path.FullName, SimplifyCubemapFace((CubemapFace)i), imageFormatParser, _qualityPercentage);
                            }
                            break;
                    }
                }
                if (imageFormat == ImageFormat.Ktx2)
                {
#if UNITY_EDITOR
                    UnityEditor.EditorUtility.DisplayProgressBar("Converting to KTX2", "Converting in progress...", 0.5f);
#endif
                    ImageUtilies.ConvertToKTX2(Map.exportSettings.OutputDirectory);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                if (cubemap != null)
                {
                    GameObject.DestroyImmediate(cubemap);
                }
                if (texture != null)
                {
                    GameObject.DestroyImmediate(texture);
                }
                #region Restore settings
                camera.transform.position = originalCameraPos;
                camera.transform.rotation = originalCameraRot;
                camera.targetTexture = camTarget;
                #endregion
            }
        }


        private Hotspot[] ValidateHotspots(TreasuredMap map)
        {
            var hotspots = map.Hotspots;
            if (hotspots == null || hotspots.Length == 0)
            {
                throw new InvalidOperationException("No active hotspots.");
            }
            return hotspots;
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
#if HDRP_10_5_1_OR_NEWER
        /// <summary>
        /// Fixes lens Lens Distortion when export cubemap in HDRP
        /// </summary>
        private Dictionary<UnityEngine.Rendering.HighDefinition.LensDistortion, float> _lensScales = new Dictionary<UnityEngine.Rendering.HighDefinition.LensDistortion, float>();

        public override void OnPreExport()
        {
            flipY = true;
            _lensScales.Clear();
            Volume[] volumes = GameObject.FindObjectsOfType<Volume>();
            for (int i = 0; i < volumes.Length; i++)
            {
                if (volumes[i].profile.TryGet<UnityEngine.Rendering.HighDefinition.LensDistortion>(out var lensDistortion))
                {
                    _lensScales[lensDistortion] = lensDistortion.scale.value;
                    lensDistortion.scale.value = 1;
                }
            }
        }

        public override void OnPostExport()
        {
            foreach (var lensDistortion in _lensScales.Keys)
            {
                if (_lensScales.TryGetValue(lensDistortion, out var scale))
                {
                    lensDistortion.scale.value = scale;
                }
            }
            _lensScales.Clear();
        }
#endif
    }
}
