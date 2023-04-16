using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Treasured.UnitySdk
{
    public enum CubemapFormat 
    { 
        IndividualFace,
        _3x2
    }

    /// <summary>
    /// Camera for the hotspot. Contains additional camera data for each Hotspot.
    /// </summary>
    public sealed class HotspotCamera : MonoBehaviour
    {
        public void Capture(Camera camera, Cubemap cubemap, Texture2D texture, string directoryPath, string defaultName, ImageQuality imageQuality, ImageFormat imageFormat, CubemapFormat cubemapFormat)
        {
            if (camera == null)
            {
                throw new System.NullReferenceException("No active camera provided");
            }
            int size = (int)imageQuality;
            string extension = imageFormat.ToString().ToLower();

            camera.transform.position = transform.position;
            camera.transform.rotation = Quaternion.identity;

            if (!camera.RenderToCubemap(cubemap))
            {
                throw new System.NotSupportedException("Current graphic device/platform does not support RenderToCubemap.");
            }
            switch (cubemapFormat)
            {
                case CubemapFormat.IndividualFace:
                    string path = $"{directoryPath}/{defaultName}.{extension}";
                    for (int i = 0; i < 6; i++)
                    {
                        texture.SetPixels(i * size, 0, size, size, cubemap.GetPixels((CubemapFace)i));
                    }
                    FlipPixels(texture, true, true);
                    File.WriteAllBytes(path, texture.EncodeToPNG());
                    break;
                case CubemapFormat._3x2:
                    // TODO: Change to 3x2 format
                    for (int i = 0; i < 6; i++)
                    {
                        path = $"{directoryPath}/{SimplifyCubemapFace((CubemapFace)i)}.{extension}";
                        texture.SetPixels(cubemap.GetPixels((CubemapFace)i));
                        FlipPixels(texture, true, true);
                        File.WriteAllBytes(path, texture.EncodeToPNG());
                    }
                    break;
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

        void OnDrawGizmosSelected()
        {
            Color tempColor = Gizmos.color;
            Matrix4x4 tempMatrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(this.transform.position, this.transform.rotation, Vector3.one);

            Gizmos.color = TreasuredSDKPreferences.Instance.frustumColor;
            Gizmos.DrawFrustum(Vector3.zero, 25, 0, 0.5f, 3);
            Gizmos.color = tempColor;
            Gizmos.matrix = tempMatrix;
        }
    }
}
