using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Treasured.UnitySdk
{
    internal class TreasuredSDKPreferencesProvider : SettingsProvider
    {
        class Styles
        {
            public static GUIContent useDefaults = new GUIContent("Use defaults");
            public static GUIContent exportSettingsLabel = new GUIContent("Export Settings");
        }

        class ExporterPreferenceGUIDrawer
        {
            public SerializedObject SerializedObject { get => Editor.serializedObject; }
            public Editor Editor { get; private set; }

            private MethodInfo _onPreferenceGUI;

            public ExporterPreferenceGUIDrawer(Exporter exporter)
            {
                this.Editor = Editor.CreateEditor(exporter);
                _onPreferenceGUI = this.Editor.GetType().GetMethod("OnPreferenceGUI", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            }

            public void OnGUI()
            {
                if (_onPreferenceGUI != null && _onPreferenceGUI.GetParameters().Length == 1)
                {
                    using (new ExporterEditor.ExporterScope(SerializedObject.FindProperty(nameof(Exporter.enabled))))
                    {
                        SerializedObject.Update();
                        EditorGUI.BeginChangeCheck();
                        _onPreferenceGUI.Invoke(this.Editor, new object[] { SerializedObject });
                        if (EditorGUI.EndChangeCheck())
                        {
                            SerializedObject.ApplyModifiedProperties();
                        }
                    }
                }
            }
        }

        SerializedObject _serializedObject;
        List<SerializedProperty> _serializedProperties;
        
        List<ExporterPreferenceGUIDrawer> _exporterDrawers = new List<ExporterPreferenceGUIDrawer>();

        public TreasuredSDKPreferencesProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
        {
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            base.OnActivate(searchContext, rootElement);
            Initialize();
        }

        void Initialize()
        {
            TreasuredSDKPreferences.Instance.Save();
            TreasuredSDKPreferences.Instance.hideFlags &= ~HideFlags.NotEditable;
            _serializedObject = TreasuredSDKPreferences.Instance.GetSerializedObject();
            var serializedFields = ReflectionUtilities.GetSerializableFieldInfoValuePair(TreasuredSDKPreferences.Instance, false);
            _serializedProperties = new List<SerializedProperty>();
            foreach (var field in serializedFields)
            {
                if (field.FieldInfo.Name.Equals("exporters")) // skip exporter[] field
                {
                    continue;
                }
                _serializedProperties.Add(_serializedObject.FindProperty(field.FieldInfo.Name));
            }
            // initialize exporter gui drawers
            _exporterDrawers = new List<ExporterPreferenceGUIDrawer>();
            for (int i = 0; i < TreasuredSDKPreferences.Instance.exporters.Count; i++)
            {
                if ((UnityEngine.Object)TreasuredSDKPreferences.Instance.exporters[i] == (UnityEngine.Object)null)
                {
                    continue;
                }
                _exporterDrawers.Add(new ExporterPreferenceGUIDrawer(TreasuredSDKPreferences.Instance.exporters[i]));
            }
        }

        public override void OnDeactivate()
        {
            base.OnDeactivate();
            TreasuredSDKPreferences.Instance.hideFlags = HideFlags.HideAndDontSave;
        }

        public override void OnGUI(string searchContext)
        {
            _serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            foreach (var serializedProperty in _serializedProperties)
            {
                if (serializedProperty.name.Equals(nameof(TreasuredSDKPreferences.customExportFolder)))
                {
                    EditorGUILayoutUtils.FolderField(serializedProperty, "Select Folder", TreasuredSDKPreferences.Instance.customExportFolder, fallbackPath: Path.Combine(Application.dataPath, "Treasured Data").Replace("\\", "/"));
                }
                else
                {
                    EditorGUILayout.PropertyField(serializedProperty);
                }
            }
            EditorGUILayout.LabelField(Styles.exportSettingsLabel, EditorStyles.boldLabel);
            foreach (var exporterDrawer in _exporterDrawers)
            {
                exporterDrawer.OnGUI();
            }
            if (GUILayout.Button(Styles.useDefaults))
            {
                TreasuredSDKPreferences.UseDefaults();
                Initialize();
            }
            if (EditorGUI.EndChangeCheck())
            {
                _serializedObject.ApplyModifiedProperties();
                TreasuredSDKPreferences.Instance.Save();
                SceneView.RepaintAll();
            }
        }

        [SettingsProvider]
        public static SettingsProvider CreateTimelineProjectSettingProvider()
        {
            var provider = new TreasuredSDKPreferencesProvider("Preferences/Treasured SDK", SettingsScope.User, GetSearchKeywordsFromGUIContentProperties<Styles>());
            return provider;
        }
    }
}
