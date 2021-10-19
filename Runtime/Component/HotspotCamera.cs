using UnityEngine;

namespace Treasured.UnitySdk
{
    /// <summary>
    /// Camera for the hotspot. Contains additional camera data for each Hotspot.
    /// </summary>
    public sealed class HotspotCamera : MonoBehaviour
    {
        public void Capture(Camera camera, string directoryPath, string extension, ImageQuality imageQuality, ImageFormat imageFormat)
        {
            if (camera == null)
            {
                throw new System.NullReferenceException("No active camera provided");
            }
            int size = (int)imageQuality;
            Vector3 position = camera.transform.position;
            Quaternion rotation = camera.transform.rotation;
            camera.transform.position = transform.position;
            camera.transform.rotation = Quaternion.identity;

        //    RenderTexture active = RenderTexture.active;
        //    RenderTexture rt = RenderTexture.GetTemporary(size, size, 0);
            Cubemap cubemap = new Cubemap(size, TextureFormat.ARGB32, false);
           // rt.dimension = UnityEngine.Rendering.TextureDimension.Cube;
            if (!camera.RenderToCubemap(cubemap))
            {
                throw new System.NotSupportedException("Current graphic device/platform does not support RenderToCubemap.");
            }
            Texture2D texture = new Texture2D(cubemap.width, cubemap.height, TextureFormat.ARGB32, false);
            for (int i = 0; i < 6; i++)
            {
                string path = $"{directoryPath}/{(CubemapFace)i}.{extension}";
                texture.SetPixels(cubemap.GetPixels((CubemapFace)i));
                byte[] bytes = texture.EncodeToPNG();
                if (bytes != null)
                {
                    System.IO.File.WriteAllBytes(path, bytes);
                }
            }
            //RenderTexture.active = rt;
            
            //texture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0, false);
            
            //RenderTexture.active = active;
            GameObject.DestroyImmediate(cubemap);
            camera.transform.position = position;
            camera.transform.rotation = rotation;
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            Color tempColor = Gizmos.color;
            Matrix4x4 tempMatrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(this.transform.position, this.transform.rotation, Vector3.one);

            Gizmos.color = TreasuredSDKSettings.Instance ? TreasuredSDKSettings.Instance.frustumColor : TreasuredSDKSettings.defaultFrustumColor;
            Gizmos.DrawFrustum(Vector3.zero, 25, 0, 0.5f, 3);
            Gizmos.color = tempColor;
            Gizmos.matrix = tempMatrix;
        }
#endif
    }
}
