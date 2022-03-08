using System.IO;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;
using System;

namespace Treasured.UnitySdk
{
    internal class TreasuredSDKSettingsProvider
    {
        public const string sdkSettingsPath = "Assets/Treasured SDK/Settings/Treasured SDK Settings.asset";
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
            var settings = AssetDatabase.LoadAssetAtPath<TreasuredSDKSettings>(sdkSettingsPath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<TreasuredSDKSettings>();
                settings.autoFocus = true;
                settings.frustumColor = TreasuredSDKSettings.DefaultFrustumColor;
                settings.hitboxColor = TreasuredSDKSettings.DefaultHitboxColor;
                string absolutePath = Path.GetFullPath(sdkSettingsPath);
                Directory.CreateDirectory(absolutePath);
                AssetDatabase.CreateAsset(settings, sdkSettingsPath);
                //var exportProcessTypes = Assembly.GetExecutingAssembly().GetTypes().Where(x => !x.IsAbstract && typeof(ExportProcess).IsAssignableFrom(x)).ToArray();
                //settings.exportProcesses = new ExportProcess[exportProcessTypes.Length];
                //for (int i = 0; i < exportProcessTypes.Length; i++)
                //{
                //    var process = (ExportProcess)ScriptableObject.CreateInstance(exportProcessTypes[i]);
                //    process.name = ObjectNames.NicifyVariableName(exportProcessTypes[i].Name);
                //    AssetDatabase.AddObjectToAsset(process, sdkSettingsPath);
                //    settings.exportProcesses[i] = process;
                //    var attribute = exportProcessTypes[i].GetCustomAttribute<ExportProcessSettingsAttribute>();
                //    if (attribute != null)
                //    {
                //        process.enabled = attribute.EnabledByDefault;
                //        //if (attribute.DisplayName == null)
                //        //{
                //        //    string typeName = type.Name;
                //        //    if (typeName.EndsWith("ExportProcess"))
                //        //    {
                //        //        typeName = typeName.Substring(0, typeName.Length - 13);
                //        //    }
                //        //    settings.DisplayName = ObjectNames.NicifyVariableName(typeName);
                //        //}
                //        //else
                //        //{
                //        //    settings.DisplayName = attribute.DisplayName;
                //        //}
                //    }
                //}
                AssetDatabase.SaveAssets();
                Debug.LogError($"Treasured SDK Settings created at \"{sdkSettingsPath}\"", settings);
            }
            return settings;
        }

        [SettingsProvider]
        internal static SettingsProvider CreateSettingsProvider()
        {
            var provider = new SettingsProvider("Preferences/Treasured SDK", SettingsScope.User, SettingsProvider.GetSearchKeywordsFromPath(sdkSettingsPath));
            FieldInfo[] serializedFields = typeof(TreasuredSDKSettingsProvider).GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            provider.guiHandler = (serachContext) =>
            {
                var settings = new SerializedObject(Settings);
                //foreach (var fi in serializedFields)
                //{
                //    if ((fi.IsPrivate && !fi.IsDefined(typeof(SerializeField))) || fi.IsDefined(typeof(HideInInspector)))
                //    {
                //        continue;
                //    }
                //    SerializedProperty serializedProperty = settings.FindProperty(fi.Name);
                //    if (serializedProperty != null)
                //    {
                //        if (serializedProperty.isArray && serializedProperty.propertyType == SerializedPropertyType.Generic)
                //        {
                //            for (int i = 0; i < serializedProperty.arraySize; i++)
                //            {
                //                var obj = serializedProperty.GetArrayElementAtIndex(i).objectReferenceValue;
                //                Type objType = obj.GetType();
                //                if (!typeof(ExportProcess).IsAssignableFrom(objType) || objType.IsDefined(typeof(HideInInspector)))
                //                {
                //                    continue;
                //                }
                //                SerializedObject serializedObject = new SerializedObject(obj);
                //                SerializedProperty enabled = serializedObject.FindProperty("enabled");
                //                MethodInfo guiMethod = objType.GetMethod("OnPreferenceGUI");
                //                enabled.boolValue = EditorGUILayout.ToggleLeft(new GUIContent(ObjectNames.NicifyVariableName(objType.Name)), enabled.boolValue, EditorStyles.boldLabel);
                //                EditorGUI.indentLevel++;
                //                guiMethod.Invoke(obj, new object[] { settings });
                //                EditorGUI.indentLevel--;
                //                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                //            }
                //        }
                //        else
                //        {
                //            EditorGUILayout.PropertyField(serializedProperty);
                //        }
                //    }
                //}
                settings.ApplyModifiedPropertiesWithoutUndo();
            };
            return provider;
        }
    }
}