#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Text;

namespace Treasured.UnitySdk
{
    internal class TreasuredSDKSettings : ScriptableObject
    {
        private static TreasuredSDKSettings _instance;
        public static TreasuredSDKSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    string path = GetSettingsPath();
                    _instance = ScriptableObject.CreateInstance<TreasuredSDKSettings>();
                    if (File.Exists(path))
                    {
                        string json = File.ReadAllText(path);
                        JsonUtility.FromJsonOverwrite(json, _instance);
                        return _instance;
                    }
                    GenerateSettingsFile(_instance);
                }
                return _instance;
            }
        }
        public static readonly Color defaultFrustumColor = Color.red;
        public static readonly Color defaultHitboxColor = new Color(0, 1, 0, 0.2f);

        [Tooltip("Auto focus on Treasured Object when being selected.")]
        public bool autoFocus = true;
        [Tooltip("Gizmos color for hotspot camera")]
        public Color frustumColor = defaultFrustumColor;
        [Tooltip("Gizmos color for hitbox")]
        public Color hitboxColor = defaultHitboxColor;

        static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(Instance);
        }

        static string GetSettingsPath()
        {
            string projectRoot = new DirectoryInfo(Application.dataPath).Parent.FullName;
            string previousSettingsPath = Path.Combine(projectRoot, "UserSettings", "TreasuredSDKSettings.json");
            if (File.Exists(previousSettingsPath))
            {
                File.Delete(previousSettingsPath);
            }
            DirectoryInfo settingsFolder = Directory.CreateDirectory(Path.Combine(projectRoot, "Treasured Data"));
            return Path.Combine(settingsFolder.FullName, "TreasuredSDKSettings.json");
        }

        static void GenerateSettingsFile(TreasuredSDKSettings settings)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(JsonUtility.ToJson(settings, true));
            File.WriteAllText(GetSettingsPath(), sb.ToString());
        }

        [SettingsProvider]
        static SettingsProvider CreateSettingsProvider()
        {
            var settings = GetSerializedSettings();
            var provider = new SettingsProvider("Preferences/Treasured SDK", SettingsScope.User, SettingsProvider.GetSearchKeywordsFromSerializedObject(settings));
            provider.guiHandler = (serachContext) =>
            {
                using (var scope = new EditorGUI.ChangeCheckScope())
                {
                    EditorGUILayout.PropertyField(settings.FindProperty(nameof(autoFocus)));
                    EditorGUILayout.PropertyField(settings.FindProperty(nameof(frustumColor)));
                    EditorGUILayout.PropertyField(settings.FindProperty(nameof(hitboxColor)));
                    if (scope.changed)
                    {
                        _instance = (TreasuredSDKSettings)settings.targetObject;
                        settings.ApplyModifiedProperties();
                    }
                }
                   
            };
            provider.deactivateHandler = () =>
            {
                _instance = (TreasuredSDKSettings)settings.targetObject;
                GenerateSettingsFile(_instance);
            };
            return provider;
        }
    }
}
#endif