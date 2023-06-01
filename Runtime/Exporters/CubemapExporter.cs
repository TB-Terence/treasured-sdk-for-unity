using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Treasured.UnitySdk.Validation;
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

        public override List<ValidationResult> CanExport()
        {
            var result = base.CanExport();
            if (Scene.Hotspots == null || Scene.Hotspots.Length == 0)
            {
                result.Add(new ValidationResult() { name = "No active hotspot(s)", description = "No active hotspot(s) found under this map object.", type = ValidationResult.ValidationResultType.Error, priority = -1 });
            }
            if (Camera.main == null)
            {
                result.Add(new ValidationResult() { name = "Camera not found", description = "Please make sure there is an active camera in the scene.", type = ValidationResult.ValidationResultType.Error, priority = -1 });
            }
            return result;
        }

        public override void Export()
        {
            var imageQualities = Enum.GetValues(typeof(ImageQuality)).Cast<ImageQuality>();

            //  Overriding export settings for Production export
            if (Scene.exportSettings.ExportType == ExportType.ProductionExport)
            {
                ExportCubemap(ImageQuality.High);
                return;
            }

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
        }

        private void ExportCubemap(ImageQuality imageQuality)
        {
            var hotspots = ValidateHotspots(Scene);
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
                    var path = Directory.CreateDirectory(Path.Combine(Scene.exportSettings.OutputDirectory, "images", exportAllQualities ? imageQuality.ToString().ToLower() : "", current.Id).ToOSSpecificPath());
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


        private Hotspot[] ValidateHotspots(TreasuredScene scene)
        {
            var hotspots = scene.Hotspots;
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

        public interface IOverwritableParameter
        {
            public bool Enabled { get; set; }
            public string FieldName { get; set; }
            public object OverwriteValue { get; set; }
            public bool GlobalOnly { get; set; }
            public void Overwrite(VolumeComponent component);
            public void Revert();
        }
        public class OverwritableParameter<T> : IOverwritableParameter
        {
            public bool Enabled { get; set; } = true;
            public string FieldName { get; set; }
            public object OverwriteValue { get; set; }
            public bool GlobalOnly { get; set; }

            VolumeComponent component;
            FieldInfo fieldInfo;
            T initialValue;

            public OverwritableParameter(string fieldName, T overwriteValue, bool globalOnly = false)
            {
                this.FieldName = fieldName;
                this.OverwriteValue = overwriteValue;
                this.GlobalOnly = globalOnly;
            }

            public void Overwrite(VolumeComponent component)
            {
                if (!Enabled) return;
                this.component = component;
                this.fieldInfo = component.GetType().GetField(FieldName);
                this.initialValue = ((VolumeParameter<T>)fieldInfo.GetValue(component)).value;
                ((VolumeParameter<T>)fieldInfo.GetValue(component)).value = (T)OverwriteValue;
                Debug.LogError($"Overwriting {FieldName} on {this.component.name} from {this.initialValue} to {this.OverwriteValue}");
            }

            public void Revert()
            {
                if (!Enabled || component == null || fieldInfo == null) return;
                ((VolumeParameter<T>)fieldInfo.GetValue(component)).value = this.initialValue;
                Debug.LogError($"Reverting {FieldName} on {this.component.name} from {this.OverwriteValue} to {this.initialValue}");
            }
        }

        public Dictionary<Type, IList<IOverwritableParameter>> OverwritableComponents = new Dictionary<Type, IList<IOverwritableParameter>>()
        {
            {
                typeof(UnityEngine.Rendering.HighDefinition.Exposure), new IOverwritableParameter[]
                {
                    new OverwritableParameter<UnityEngine.Rendering.HighDefinition.ExposureMode>(
                        nameof(UnityEngine.Rendering.HighDefinition.Exposure.mode),
                        UnityEngine.Rendering.HighDefinition.ExposureMode.Fixed, true),
                }
            },
            {
                typeof(UnityEngine.Rendering.HighDefinition.LensDistortion), new IOverwritableParameter[]
                {
                    new OverwritableParameter<float>(nameof(UnityEngine.Rendering.HighDefinition.LensDistortion.scale), 1),
                    new OverwritableParameter<float>("intensity", 0),
                }
            },
            {
                typeof(UnityEngine.Rendering.HighDefinition.AmbientOcclusion), new IOverwritableParameter[]
                {
                    new OverwritableParameter<float>("intensity", 0)
                }
            }
            ,
            {
                typeof(UnityEngine.Rendering.HighDefinition.Bloom), new IOverwritableParameter[]
                {
                    new OverwritableParameter<float>("intensity", 0)
                }
            },
            {
                typeof(UnityEngine.Rendering.HighDefinition.MotionBlur), new IOverwritableParameter[]
                {
                    new OverwritableParameter<float>("intensity", 0)
                }
            },
            {
                typeof(UnityEngine.Rendering.HighDefinition.FilmGrain), new IOverwritableParameter[]
                {
                    new OverwritableParameter<float>("intensity", 0)
                }
            },
            {
                typeof(UnityEngine.Rendering.HighDefinition.ChromaticAberration), new IOverwritableParameter[]
                {
                    new OverwritableParameter<float>("intensity", 0)
                }
            },
            {
                typeof(UnityEngine.Rendering.HighDefinition.DepthOfField), new IOverwritableParameter[]
                {
                    new OverwritableParameter<float>(nameof(UnityEngine.Rendering.HighDefinition.DepthOfField.farMaxBlur), 0),
                    new OverwritableParameter<float>(nameof(UnityEngine.Rendering.HighDefinition.DepthOfField.nearMaxBlur), 0)
                }
            }
        };

        public override void OnPreExport()
        {
            string path = Path.Combine(Scene.exportSettings.OutputDirectory, "images");
            if (Directory.Exists(path))
                Directory.Delete(path, true);
            flipY = true;
            //_parameterOverwrites.Clear();
            _cameraData = ValidateCamera().gameObject.GetComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData>();
            _antiAliasing = _cameraData.antialiasing;
            _cameraData.antialiasing = UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData.AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            _SMAAQuality = _cameraData.SMAAQuality;
            _cameraData.SMAAQuality = UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData.SMAAQualityLevel.High;
            foreach (var volume in GameObject.FindObjectsOfType<Volume>())
            {
                foreach (var component in volume.profile.components)
                {
                    if (OverwritableComponents.TryGetValue(component.GetType(), out var paramenters))
                    {
                        foreach (var parameter in paramenters)
                        {
                            parameter.Overwrite(component);
                        }
                    }
                }
            }
        }

        private void AddOverwrite<T>(OverwritableParameter<T> overwrite)
        {
            if (!OverwritableComponents.ContainsKey(typeof(T)))
            {
                OverwritableComponents[typeof(T)] = new List<IOverwritableParameter>();
            }
            OverwritableComponents[typeof(T)].Add(overwrite);
        }

        public override void OnPostExport()
        {
            if (_cameraData)
            {
                _cameraData.antialiasing = _antiAliasing;
                _cameraData.SMAAQuality = _SMAAQuality;
            }
            foreach (var overwrites in OverwritableComponents.Values)
            {
                foreach (var overwrite in overwrites)
                {
                    overwrite.Revert();
                }
            }
        }
#endif
    }
}