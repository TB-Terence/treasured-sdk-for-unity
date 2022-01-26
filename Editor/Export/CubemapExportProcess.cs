using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    internal class CubemapExportProcess : ExportProcess
    {
        private const int CUDA_TEXTURE_WIDTH = 4096;
        private const int CUBEMAP_FACE_WIDTH = 1360; // 4096 / 3 round to nearest tenth

        private SerializedProperty _format;
        private SerializedProperty _quality;
        private CubemapFormat cubemapFormat = CubemapFormat.Single;
        private int qualityPercentage = 75;
        private ImageFormat imageFormat = ImageFormat.Ktx2;

        public override void OnEnable(SerializedObject serializedObject)
        {
            _format = serializedObject.FindProperty(nameof(_format));
            _quality = serializedObject.FindProperty(nameof(_quality));
            _format.enumValueIndex = (int)ImageFormat.Ktx2;
            serializedObject.ApplyModifiedProperties();
        }
        public override void OnGUI(SerializedObject serializedObject)
        {
            
            //EditorGUILayout.PropertyField(_format);
            imageFormat = (ImageFormat)EditorGUILayout.EnumPopup(new GUIContent("Format"), imageFormat);
            EditorGUILayout.PropertyField(_quality);
            if (_format.enumValueIndex == (int)ImageFormat.PNG || _format.enumValueIndex == (int)ImageFormat.Ktx2)
                return;

            using (new EditorGUILayout.HorizontalScope())
            {
                qualityPercentage = EditorGUILayout.IntSlider(new GUIContent("Quality Percentage"), qualityPercentage, 1, 100);
                GUILayout.Label("%");
            }
        }

        public override void Export(string rootDirectory, TreasuredMap map)
        {
            var hotspots = ValidateHotspots(map);
            var camera = ValidateCamera(); // use default camera settings to render 360 images

            #region Get camera settings
            RenderTexture camTarget = camera.targetTexture;
            Vector3 originalCameraPos = camera.transform.position;
            Quaternion originalCameraRot = camera.transform.rotation;
            #endregion

            //int size = 1024;//Mathf.Min(Mathf.NextPowerOfTwo((int)map.Quality), 8192);

            int count = hotspots.Length;

            Cubemap cubemap = new Cubemap(CUBEMAP_FACE_WIDTH, TextureFormat.ARGB32, false);
            Texture2D texture = null;
            switch (cubemapFormat)
            {
                case CubemapFormat.Single:
                    texture = new Texture2D(CUDA_TEXTURE_WIDTH, CUDA_TEXTURE_WIDTH, TextureFormat.ARGB32, false);
                    break;
                case CubemapFormat.SixFaces:
                    texture = new Texture2D(cubemap.width, cubemap.height, TextureFormat.ARGB32, false);
                    break;
            };
            //  If imageFormat is KTX2 then export images as png and then later convert them to KTX2 format  
            ImageFormat imageFormatParser = (imageFormat == ImageFormat.Ktx2) ? ImageFormat.PNG : imageFormat;

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
                    var path = Directory.CreateDirectory(Path.Combine(rootDirectory, "images", current.Id).Replace('/', '\\'));
                    switch (cubemapFormat)
                    {
                        case CubemapFormat.Single:
                            // FORMAT:
                            // RIGHT(+X) LEFT(-X) TOP(+Y)
                            // BOTTOM(-Y) FRONT(+Z) BACK(-Z)
                            for (int i = 0; i < 6; i++)
                            {
                                if (EditorUtility.DisplayCancelableProgressBar(progressTitle, progressText, i / 6f))
                                {
                                    throw new TreasuredException("Export canceled", "Export canceled by the user.");
                                }
                                texture.SetPixels((i % 3) * CUBEMAP_FACE_WIDTH, 4096 - ((i / 3) + 1) * CUBEMAP_FACE_WIDTH, CUBEMAP_FACE_WIDTH, CUBEMAP_FACE_WIDTH,
                                ImageUtilies.FlipPixels(cubemap.GetPixels((CubemapFace)i), CUBEMAP_FACE_WIDTH, CUBEMAP_FACE_WIDTH, true, imageFormat != ImageFormat.Ktx2));
                            }
                            ImageUtilies.Encode(texture, path.FullName, "cubemap", imageFormatParser, qualityPercentage);
                            break;
                        case CubemapFormat.SixFaces:
                            for (int i = 0; i < 6; i++)
                            {
                                if (EditorUtility.DisplayCancelableProgressBar(progressTitle, progressText, i / 6f))
                                {
                                    throw new TreasuredException("Export canceled", "Export canceled by the user.");
                                }
                                texture.SetPixels(cubemap.GetPixels((CubemapFace)i));
                                ImageUtilies.FlipPixels(texture, true, imageFormat != ImageFormat.Ktx2);
                                ImageUtilies.Encode(texture, path.FullName, SimplifyCubemapFace((CubemapFace)i), imageFormatParser, qualityPercentage);
                            }
                            break;
                    }
                }
                if (imageFormat == ImageFormat.Ktx2)
                {
                    EditorUtility.DisplayProgressBar("Converting to KTX2", "Converting in progress...", 0.5f);
                    ImageUtilies.ConvertToKTX2(rootDirectory);
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
    }
}
