using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
        public bool exportAllQualities = true;
        public ImageQuality imageQuality = ImageQuality.High;
        public CubemapFormat cubemapFormat = CubemapFormat.IndividualFace;
        [SerializeField]
        private bool _useCustomWidth = false;
        [SerializeField]
        private int _customCubemapWidth = MAXIMUM_CUBEMAP_FACE_WIDTH;
        [HideInInspector]
        public bool flipY = true;

        [SerializeField]
        [HideInInspector]
        private int _qualityPercentage = 75;

        [UnityEngine.ContextMenu("Reset")]
        private void Reset()
        {
            enabled = true;
            imageFormat = ImageFormat.Ktx2;
            exportAllQualities = true;
            imageQuality = ImageQuality.High;
            cubemapFormat = CubemapFormat.IndividualFace;
            _useCustomWidth = false;
            _customCubemapWidth = MAXIMUM_CUBEMAP_FACE_WIDTH;
            flipY = true;
            _qualityPercentage = 75;
        }

        public override void Export()
        {
            var imageQualities = Enum.GetValues(typeof(ImageQuality)).Cast<ImageQuality>();

            if (exportAllQualities)
            {
                foreach (var quality in imageQualities)
                {
                    ExportCubemap(quality);
                }
            }
            else
            {
                ExportCubemap(imageQuality);
            }

            if (Map.exportSettings.optimizeScene)
            {
                ImageUtilies.ConvertToKTX2(Map.exportSettings.OutputDirectory);
            }
        }

        private void ExportCubemap(ImageQuality imageQuality)
        {
            var hotspots = ValidateHotspots(Map);
            var camera = ValidateCamera(); // use default camera settings to render 360 images

            #region Get camera settings
            RenderTexture camTarget = camera.targetTexture;
            Vector3 originalCameraPos = camera.transform.position;
            Quaternion originalCameraRot = camera.transform.rotation;
            #endregion

            int count = hotspots.Length;
            
            Cubemap cubemap = new Cubemap(_useCustomWidth ? _customCubemapWidth : (int)imageQuality, TextureFormat.ARGB32, false);
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
                    string progressTitle = $"Capturing Hotspots";
                    string progressText = $"{current.name}";

                    camera.transform.SetPositionAndRotation(current.Camera.transform.position, Quaternion.identity);

                    if (!camera.RenderToCubemap(cubemap))
                    {
                        throw new System.NotSupportedException("Current graphic device/platform does not support RenderToCubemap.");
                    }
                    var path = Directory.CreateDirectory(Path.Combine(Map.exportSettings.OutputDirectory, "images", exportAllQualities ? imageQuality.ToString().ToLower() : "", current.Id).ToOSSpecificPath());
                    switch (cubemapFormat)
                    {
                        case CubemapFormat._3x2:
                            // FORMAT:
                            // RIGHT(+X) LEFT(-X) TOP(+Y)
                            // BOTTOM(-Y) FRONT(+Z) BACK(-Z)
                            int cubemapWidth = Mathf.Clamp(_useCustomWidth ? _customCubemapWidth - _customCubemapWidth % 10 : (int)imageQuality, 16, CubemapExporter.MAXIMUM_CUBEMAP_FACE_WIDTH);
                            for (int i = 0; i < 6; i++)
                            {
#if UNITY_EDITOR
                                if (UnityEditor.EditorUtility.DisplayCancelableProgressBar(progressTitle, progressText, i / 6f))
                                {
                                    throw new OperationCanceledException("Export canceled by user.");
                                }
#endif
                                texture.SetPixels((i % 3) * cubemapWidth, MAXIMUM_CUDA_TEXTURE_WIDTH - ((i / 3) + 1) * cubemapWidth, cubemapWidth, cubemapWidth,
                                ImageUtilies.FlipPixels(cubemap.GetPixels((CubemapFace)i), cubemapWidth, cubemapWidth, true, flipY));
                            }
                            ImageUtilies.Encode(texture, path.FullName, "cubemap", imageFormatParser, _qualityPercentage);
                            break;
                        case CubemapFormat.IndividualFace:
                            for (int i = 0; i < 6; i++)
                            {
#if UNITY_EDITOR
                                if (UnityEditor.EditorUtility.DisplayCancelableProgressBar(progressTitle, progressText, i / 6f))
                                {
                                    throw new OperationCanceledException("Export canceled by user.");
                                }
#endif
                                texture.SetPixels(cubemap.GetPixels((CubemapFace)i));
                                ImageUtilies.FlipPixels(texture, true, flipY);
                                ImageUtilies.Encode(texture, path.FullName, SimplifyCubemapFace((CubemapFace)i), imageFormatParser, _qualityPercentage);
                            }
                            break;
                    }
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
        private UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData _cameraData;
        private UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData.AntialiasingMode _antiAliasing;
        private UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData.SMAAQualityLevel _SMAAQuality;
        private Dictionary<VolumeComponent, List<IParameterOverwrite>> _parameterOverwrites = new Dictionary<VolumeComponent, List<IParameterOverwrite>>();

        private interface IParameterOverwrite
        {
            public VolumeComponent Component { get; }
            public FieldInfo FieldInfo { get; }
            public object OverwriteValue { get; set; }
            public void Overwrite();
            public void Revert();
        }
        private struct ParameterOverwrite<T> : IParameterOverwrite
        {
            /// <summary>
            /// <see cref="VolumeComponent"/> on HDRP profile to overwrite.
            /// </summary>
            public VolumeComponent Component { get; private set; }
            /// <summary>
            /// Field Info of the <see cref="VolumeParameter"/>
            /// </summary>
            public FieldInfo FieldInfo { get; private set; }
            public T InitialValue { get; private set; }
            public object OverwriteValue { get; set; }
            
            public ParameterOverwrite(VolumeComponent component, string fieldName, T overwriteValue)
            {
                this.Component = component;
                this.FieldInfo = component.GetType().GetField(fieldName);
                this.OverwriteValue = overwriteValue;
                this.InitialValue = ((VolumeParameter<T>)this.FieldInfo.GetValue(component)).value;
            }

            public void Overwrite()
            {
                ((VolumeParameter<T>)FieldInfo.GetValue(Component)).value = (T)OverwriteValue;
            }

            public void Revert()
            {
                ((VolumeParameter<T>)FieldInfo.GetValue(Component)).value = InitialValue;
            }

            public override int GetHashCode()
            {
                return Component.GetHashCode();
            }
        }

        public override void OnPreExport()
        {
            flipY = true;
            _parameterOverwrites.Clear();
            _cameraData = ValidateCamera().gameObject.GetComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData>();
            _antiAliasing = _cameraData.antialiasing;
            _cameraData.antialiasing = UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData.AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            _SMAAQuality = _cameraData.SMAAQuality;
            _cameraData.SMAAQuality = UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData.SMAAQualityLevel.High;
            foreach (var volume in GameObject.FindObjectsOfType<Volume>())
            {
                foreach (var component in volume.profile.components)
                {
                    switch (component)
                    {
                        case UnityEngine.Rendering.HighDefinition.Exposure e:
                            if(volume.isGlobal)
                                AddParameterOverwrite(new ParameterOverwrite<UnityEngine.Rendering.HighDefinition.ExposureMode>(component, nameof(UnityEngine.Rendering.HighDefinition.Exposure.mode), UnityEngine.Rendering.HighDefinition.ExposureMode.Fixed));
                            break;
                        case UnityEngine.Rendering.HighDefinition.LensDistortion ld:
                            AddParameterOverwrite(new ParameterOverwrite<float>(component, nameof(UnityEngine.Rendering.HighDefinition.LensDistortion.scale), 1));
                            AddParameterOverwrite(new ParameterOverwrite<float>(component, nameof(UnityEngine.Rendering.HighDefinition.LensDistortion.intensity), 0));
                            break;
                        // intensity
                        case UnityEngine.Rendering.HighDefinition.AmbientOcclusion ao:
                        case UnityEngine.Rendering.HighDefinition.Bloom b:
                        case UnityEngine.Rendering.HighDefinition.MotionBlur mb:
                        case UnityEngine.Rendering.HighDefinition.FilmGrain fg:
                        case UnityEngine.Rendering.HighDefinition.Vignette v:
                        case UnityEngine.Rendering.HighDefinition.ChromaticAberration ca:
                            AddParameterOverwrite(new ParameterOverwrite<float>(component, "intensity", 0));
                            break;
                        case UnityEngine.Rendering.HighDefinition.DepthOfField dof:
                            if(dof.focusMode == UnityEngine.Rendering.HighDefinition.DepthOfFieldMode.UsePhysicalCamera)
                            {
                                AddParameterOverwrite(new ParameterOverwrite<UnityEngine.Rendering.HighDefinition.DepthOfFieldMode>(component, nameof(UnityEngine.Rendering.HighDefinition.DepthOfField.farMaxBlur), 0));
                                AddParameterOverwrite(new ParameterOverwrite<UnityEngine.Rendering.HighDefinition.DepthOfFieldMode>(component, nameof(UnityEngine.Rendering.HighDefinition.DepthOfField.nearMaxBlur), 0));
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            
        }

        private void AddParameterOverwrite<T>(ParameterOverwrite<T> overwrite)
        {
            if (!_parameterOverwrites.ContainsKey(overwrite.Component))
            {
                _parameterOverwrites[overwrite.Component] = new List<IParameterOverwrite>();
            }
            _parameterOverwrites[overwrite.Component].Add(overwrite);
            overwrite.Overwrite();
        }

        public override void OnPostExport()
        {
            if(_cameraData)
            {
                _cameraData.antialiasing = _antiAliasing;
                _cameraData.SMAAQuality = _SMAAQuality;
            }
            foreach (var overwrites in _parameterOverwrites.Values)
            {
                foreach (var overwrite in overwrites)
                {
                    overwrite.Revert();
                }
            }
            _parameterOverwrites.Clear();
        }
#endif
    }
}
