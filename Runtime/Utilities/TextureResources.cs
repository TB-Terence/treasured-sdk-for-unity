using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Treasured.UnitySdk
{
    internal static class TextureResources
    {
        private static Dictionary<string, Texture> s_textures = new Dictionary<string, Texture>();

        public static bool TryGetTexture(string key, out Texture texture)
        {
            return s_textures.TryGetValue(key, out texture);
        }

        public static void AddTexture(string key, Texture texture)
        {
            s_textures[key] = (texture as Texture2D).ApplyTint(Color.white);
        }

        public static Texture2D ApplyTint(this Texture2D texture, Color tint)
        {
            Color[] colors = texture.GetPixels();
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] *= tint;
            }
            texture.SetPixels(colors);
            texture.Apply();
            return texture;
        }
    }
}
