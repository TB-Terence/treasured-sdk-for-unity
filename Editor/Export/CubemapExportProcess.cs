using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    internal class CubemapExportProcess : ExportProcess
    {
        private CubemapFormat cubemapFormat = CubemapFormat.SixFaces;
        private int qualityPercentage = 75;

        public override void OnGUI(SerializedObject serializedObject)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_quality"));
            using (new EditorGUILayout.HorizontalScope())
            {
                qualityPercentage = EditorGUILayout.IntSlider(new GUIContent("Quality Percentage"), qualityPercentage, 1, 100);
                EditorGUILayout.LabelField("%", GUILayout.Width(48));
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

            int size = Mathf.Min(Mathf.NextPowerOfTwo((int)map.Quality), 8192);

            int count = hotspots.Length;

            Cubemap cubemap = new Cubemap(size, TextureFormat.ARGB32, false);
            Texture2D texture = new Texture2D(cubemap.width * (cubemapFormat == CubemapFormat.Single ? 6 : 1), cubemap.height, TextureFormat.ARGB32, false);
            ImageFormat imageFormat = ImageFormat.WEBP;

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
                    var path = Directory.CreateDirectory(Path.Combine(rootDirectory, "images", current.Id));
                    switch (cubemapFormat)
                    {
                        case CubemapFormat.Single:
                            for (int i = 0; i < 6; i++)
                            {
                                if (EditorUtility.DisplayCancelableProgressBar(progressTitle, progressText, i / 6f))
                                {
                                    throw new TreasuredException("Export canceled", "Export canceled by the user.");
                                }
                                texture.SetPixels(i * size, 0, size, size, cubemap.GetPixels((CubemapFace)i));
                            }
                            ImageUtilies.FlipPixels(texture, true, true);
                            ImageUtilies.Encode(texture, path.FullName, "cubemap", imageFormat, qualityPercentage);
                            break;
                        case CubemapFormat.SixFaces:
                            for (int i = 0; i < 6; i++)
                            {
                                if (EditorUtility.DisplayCancelableProgressBar(progressTitle, progressText, i / 6f))
                                {
                                    throw new TreasuredException("Export canceled", "Export canceled by the user.");
                                }
                                texture.SetPixels(cubemap.GetPixels((CubemapFace)i));
                                ImageUtilies.FlipPixels(texture, true, true);
                                ImageUtilies.Encode(texture, path.FullName, SimplifyCubemapFace((CubemapFace)i), imageFormat, qualityPercentage);
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
    }
}
