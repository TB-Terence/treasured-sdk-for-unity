using System.IO;
using System.Reflection;
using Treasured.UnitySdk.Utilities;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    internal class TreasuredSDKSettingsProvider
    {
        public const string DefaultSdkSettingsDirectory = "Assets/Treasured SDK/Settings/";
        public const string DefaultSdkSettingsFileName = "Settings.asset";

        private static TreasuredSDKSettings _settings;
        public static TreasuredSDKSettings Settings
        {
            get
            {
                if (_settings == null)
                {
                    _settings = GetOrCreateSettings();
                }
                return _settings;
            }
        }
        
        internal static TreasuredSDKSettings GetOrCreateSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<TreasuredSDKSettings>(DefaultSdkSettingsDirectory);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<TreasuredSDKSettings>();
                settings.autoFocus = true;
                settings.frustumColor = TreasuredSDKSettings.DefaultFrustumColor;
                settings.hitboxColor = TreasuredSDKSettings.DefaultHitboxColor;
                string absolutePath = Path.GetFullPath(DefaultSdkSettingsDirectory);
                if (!Directory.Exists(absolutePath))
                {
                    Directory.CreateDirectory(absolutePath);
                }
                AssetDatabase.CreateAsset(settings, Path.Combine(DefaultSdkSettingsDirectory, DefaultSdkSettingsFileName));
                AssetDatabase.SaveAssets();
            }
            return settings;
        }

        [SettingsProvider]
        internal static SettingsProvider CreateSettingsProvider()
        {
            var provider = new SettingsProvider("Preferences/Treasured SDK", SettingsScope.User, SettingsProvider.GetSearchKeywordsFromPath(DefaultSdkSettingsDirectory));
            FieldInfo[] serializedFields = typeof(TreasuredSDKSettingsProvider).GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            provider.guiHandler = (serachContext) =>
            {
                var settings = new SerializedObject(Settings);
                EditorGUIUtils.DrawPropertiesExcluding(settings, "m_Script");
                settings.ApplyModifiedPropertiesWithoutUndo();
            };
            return provider;
        }
    }
}