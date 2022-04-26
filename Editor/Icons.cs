using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    public sealed class Icons
    {
        public const string Filter = "t:texture2D";
        public static readonly string[] IconDirectories = new string[] { "Assets/Treasured SDK/Icons/", "Packages/com.treasured.unitysdk/Resources/Icons" };

        public static List<IconAsset> BuiltInIcons;
        public static List<IconAsset> CustomIcons;

        public static IconAsset Default { get; private set; }

        [InitializeOnLoadMethod]
        public static void Init()
        {
            CreateIconAssets();
        }

        [MenuItem("Tools/Treasured/Icons/Create Icon Assets")]
        public static void CreateIconAssets()
        {
            EnsureCusomtIconDirectoryExist();
            LoadIcons(ref CustomIcons, new string[] { IconDirectories[0] });
            LoadIcons(ref BuiltInIcons, new string[] { IconDirectories[1] });
        }

        [MenuItem("Tools/Treasured/Icons/Remove All Icon Asset")]
        public static void RemoveAllIconAsset()
        {
            string[] guids = AssetDatabase.FindAssets(Filter, IconDirectories);
            for (int i = 0; i < guids.Length; i++)
            {
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(guids[i]));
                if (RemoveAssetForTexture(texture))
                {
                    Debug.Log(texture.name + " removed");
                }
            }
            BuiltInIcons.Clear();
            AssetDatabase.Refresh();
        }


        public static void LoadIcons(ref List<IconAsset> icons, string[] directories)
        {
            icons ??= new List<IconAsset>();
            icons.Clear();
            var guids = AssetDatabase.FindAssets(Filter, directories);
            for (int i = 0; i < guids.Length; i++)
            {
                string guid = guids[i];
                string texturePath = AssetDatabase.GUIDToAssetPath(guid);
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                string assetPath = @$"{Path.GetDirectoryName(texturePath)}/{texture.name}.asset".Replace("\\", "/");
                IconAsset asset = AssetDatabase.LoadAssetAtPath<IconAsset>(assetPath);
                // Create Icon asset if not exist
                if (asset == null)
                {
                    asset = ScriptableObject.CreateInstance<IconAsset>();
                    AssetDatabase.CreateAsset(asset, assetPath);
                }
                // Assign/Reassign icon texture
                if (asset.icon != texture)
                {
                    asset.icon = texture;
                }
                icons.Add(asset);
                // Set Default Icon for Object Picker
                if (texture.name.Equals("Default") && assetPath.StartsWith(IconDirectories[1]))
                {
                    Default = asset;
                }
            }
        }

        /// <summary>
        /// Removes IconAsset for a texture if exist.
        /// </summary>
        /// <param name="texture"></param>
        /// <returns>Return true if icon asset for the texture is removed. Otherwise false.</returns>
        public static bool RemoveAssetForTexture(Texture2D texture)
        {
            string texturePath = AssetDatabase.GetAssetPath(texture);
            if (string.IsNullOrEmpty(texturePath))
            {
                Debug.LogError("Texture not found");
                return false;
            }
            string assetPath = $@"{Path.GetDirectoryName(texturePath)}\{texture.name}.asset";
            return AssetDatabase.DeleteAsset(assetPath);
        }

        public static void EnsureCusomtIconDirectoryExist()
        {
            if (!AssetDatabase.IsValidFolder(@"Assets\Treasured SDK"))
            {
                AssetDatabase.CreateFolder("Assets", "Treasured SDK");
            }
            if (!AssetDatabase.IsValidFolder(@"Assets\Treasured SDK\Icons"))
            {
                AssetDatabase.CreateFolder(@"Assets\Treasured SDK", "Icons");
            }
        }
    }
}
