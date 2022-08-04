using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    public sealed class InteractableButtonIconProvider
    {
        private const string k_CustomIconsFolderKey = "TreasuredSDK_InteractableButtonIcons_CustomFolderPath";

        public const string Filter = "t:Treasured.UnitySdk.IconAsset";
        public static readonly string[] IconDirectories = new string[] { "Assets/Treasured SDK/Icons/", "Packages/com.treasured.unitysdk/Resources/Icons" };
        private static Dictionary<string, IconAsset> s_icons = new Dictionary<string, IconAsset>();


        /// <summary>
        /// Return the Icon Asset by name(case sensitive).
        /// </summary>
        /// <param name="name"></param>
        /// <param name="asset"></param>
        public static void TryGetIconAssetByName(string name, out IconAsset asset)
        {
            asset = s_icons.Values.FirstOrDefault(asset => asset.name.Equals(name));
        }

        /// <summary>
        /// Returns the Custom Icon Folder path of current Unity Session. Helpful for selecting same folder over and over again.
        /// </summary>
        public static string CustomIconFolderOfCurrentSession
        {
            get
            {
                return SessionState.GetString(k_CustomIconsFolderKey, CustomIconFolder);
            }
            set
            {
                SessionState.SetString(k_CustomIconsFolderKey, value);
            }
        }

        /// <summary>
        /// Returns the Custom Icon Folder path, <see cref="Application.dataPath"/> will be returned for if not set.
        /// </summary>
        public static string CustomIconFolder
        {
            get
            {
                return EditorPrefs.GetString(k_CustomIconsFolderKey, Application.dataPath);
            }
            set
            {
                EditorPrefs.SetString(k_CustomIconsFolderKey, value);
            }
        }

        /// <summary>
        /// The default Icon.
        /// </summary>
        public static IconAsset Default { get; private set; }

        /// <summary>
        /// Load Icon Asset into memory.
        /// </summary>
        [InitializeOnLoadMethod]
        static void Init()
        {
            CreateCusomtIconDirectoryIfNotExist();
            LoadIconAssets(IconDirectories);
        }

        /// <summary>
        /// Import icon asset from image file. Currently only PNG is supported. This will import IconAsset that have the same Texture2D.imageContentsHash.
        /// </summary>
        /// <param name="texturePath"></param>
        /// <returns></returns>
        public static IconAsset ImportIconAsset(string texturePath)
        {
            if (string.IsNullOrEmpty(texturePath))
            {
                return null;
            }
#if !UNITY_EDITOR
            return null;
#endif
            EditorUtility.DisplayProgressBar("Importing Icon", $"{texturePath}", 0);
            string name = Path.GetFileNameWithoutExtension(texturePath);
            Texture2D texture = new Texture2D(2, 2);
            texture.name = name;
            try
            {
                texture.LoadImage(File.ReadAllBytes(texturePath));
            }
            catch(Exception e)
            {
                throw new Exception($"Unable to Import Icon due to {e.Message}");
            }
            // Skip icon with same content
            if (s_icons.Values.Any(asset => asset.icon.imageContentsHash.Equals(texture.imageContentsHash)))
            {
                return null;
            }
            IconAsset iconAsset = ScriptableObject.CreateInstance<IconAsset>();
            iconAsset.icon = texture;
            AssetDatabase.CreateAsset(iconAsset, @$"Assets\Treasured SDK\Icons\{name}.asset");
            AssetDatabase.AddObjectToAsset(texture, iconAsset);
            EditorUtility.ClearProgressBar();
            s_icons.Add(AssetDatabase.GetAssetPath(iconAsset), iconAsset);
            return iconAsset;
        }

        /// <summary>
        /// Import Icon Asset from an image file.
        /// </summary>
        /// <returns></returns>
        public static IconAsset ImportIconAsset()
        {
            string texturePath = EditorUtility.OpenFilePanel("Select Texture File", CustomIconFolderOfCurrentSession, "png");
            return ImportIconAsset(texturePath);
        }

        /// <summary>
        /// Import Icon Assets from image files under <paramref name="folderPath"/>.
        /// </summary>
        /// <param name="folderPath"></param>
        public static void ImportIconAssetsFromFolder(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                return;
            }
            DirectoryInfo customFolder = new DirectoryInfo(@$"{Application.dataPath}\Treasured SDK\Icons");
            DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);
            var files = directoryInfo.GetFiles().Where(file => file.Extension.ToLowerInvariant().Equals(".png")).ToArray();
            for (int i = 0; i < files.Length; i++)
            {
                FileInfo file = files[i];
                ImportIconAsset(file.FullName);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
        }

        private static void LoadIconAssets(string[] directories)
        {
            s_icons ??= new Dictionary<string, IconAsset>();
            s_icons.Clear();
            var guids = AssetDatabase.FindAssets(Filter, directories);
            for (int i = 0; i < guids.Length; i++)
            {
                string guid = guids[i];
                string iconAssetPath = AssetDatabase.GUIDToAssetPath(guid);
                IconAsset asset = AssetDatabase.LoadAssetAtPath<IconAsset>(iconAssetPath);
                // Set Default Icon for Object Picker
                if (asset.name.Equals("Default") && iconAssetPath.StartsWith(IconDirectories[1]))
                {
                    Default = asset;
                }
                s_icons.Add(AssetDatabase.GetAssetPath(asset), asset);
            }
        }

        /// <summary>
        /// Creates Custom Icon Folder under Assets/Treasured SDK/Icons
        /// </summary>
        private static void CreateCusomtIconDirectoryIfNotExist()
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

        class IconModificationProcessor : UnityEditor.AssetModificationProcessor
        {
            /// <summary>
            /// Clean up the Icon stored in FloatingIconProvider.
            /// </summary>
            /// <param name="assetPath"></param>
            /// <param name="options"></param>
            /// <returns></returns>
            static AssetDeleteResult OnWillDeleteAsset(string assetPath, RemoveAssetOptions options)
            {
                if (s_icons.ContainsKey(assetPath))
                {
                    s_icons.Remove(assetPath);
                }
                return AssetDeleteResult.DidNotDelete;
            }
        }
    }
}
