﻿#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEditor;

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
                    string path = GetAbsolutePath();
                    _instance = ScriptableObject.CreateInstance<TreasuredSDKSettings>();
                    if (File.Exists(path))
                    {
                        string json = File.ReadAllText(path);
                        JsonUtility.FromJsonOverwrite(json, _instance);
                        return _instance;
                    }
                    string newJson = JsonUtility.ToJson(_instance, true);
                    File.WriteAllText(path, newJson);
                }
                return _instance;
            }
        }
        public static readonly Color defaultFrustumColor = Color.white;
        public static readonly Color defaultHitboxColor = new Color(0, 1, 0, 0.2f);
        class Styles
        {
            public static readonly GUIContent frustumColor = EditorGUIUtility.TrTextContent("Frustum Color");
            public static readonly GUIContent hitboxColor = EditorGUIUtility.TrTextContent("Hitbox Color");
        }

        public Color frustumColor = defaultFrustumColor;
        public Color hitboxColor = defaultHitboxColor;

        static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(Instance);
        }

        static string GetAbsolutePath()
        {
            return Path.Combine(new DirectoryInfo(Application.dataPath).Parent.FullName, "UserSettings", "TreasuredSDKSettings.json");
        }

        [SettingsProvider]
        static SettingsProvider CreateSettingsProvider()
        {
            var provider = new SettingsProvider("Preferences/Treasured SDK", SettingsScope.User, SettingsProvider.GetSearchKeywordsFromGUIContentProperties<Styles>());
            var settings = GetSerializedSettings();
            provider.guiHandler = (serachContext) =>
            {
                using (var scope = new EditorGUI.ChangeCheckScope())
                {
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
                string newJson = JsonUtility.ToJson(Instance, true);
                File.WriteAllText(GetAbsolutePath(), newJson);
            };
            return provider;
        }
    }
}
#endif