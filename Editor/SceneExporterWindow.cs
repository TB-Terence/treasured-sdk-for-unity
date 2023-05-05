using System;
using System.Collections.Generic;
using System.Linq;
using Treasured.UnitySdk.Validation;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    internal sealed class SceneExporterWindow : UnityEditor.EditorWindow
    {
        class Styles
        {
            public static GUIContent[] tabs = new GUIContent[] { EditorGUIUtility.TrIconContent("console.infoicon"), EditorGUIUtility.TrIconContent("warning"), EditorGUIUtility.TrIconContent("error") };
            public static GUIStyle selected = new GUIStyle(EditorStyles.toolbarButton)
            {
                normal = EditorStyles.toolbarButton.active,
                onNormal = EditorStyles.toolbarButton.active,
                onFocused = EditorStyles.toolbarButton.active,
                onHover = EditorStyles.toolbarButton.active,
            };
            public static GUIStyle deselected = new GUIStyle(EditorStyles.toolbarButton);
        }

        public class ListItem
        {
            public bool expanded = true;
            public ValidationResult validationResult;
        }

        public TreasuredScene scene;
        public List<ListItem> results = new List<ListItem>();

        private ValidationResult.ValidationResultType _type = ValidationResult.ValidationResultType.Warning | ValidationResult.ValidationResultType.Error | ValidationResult.ValidationResultType.Info;
        private Vector2 _scrollPosition;
        private bool hasError = false;

        public static void Show(TreasuredScene scene, ValidationException e)
        {
            var window = EditorWindow.GetWindow<SceneExporterWindow>(true, "Scene Exporter", true);
            window.scene = scene;
            window.results = e.results.Select(result => new ListItem() { validationResult = result}).OrderBy(x => x.validationResult.priority).ToList();
            Styles.tabs[0].text = $"({e.infos.Count})";
            Styles.tabs[1].text = $"({e.warnings.Count})";
            Styles.tabs[2].text = $"({e.errors.Count})";
            window.hasError = e.infos.Count > 0;
            window.Show();
        }

        private void OnGUI()
        {
            DefaultStyles.Init();
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.FlexibleSpace();
                for (int i = 0; i < Styles.tabs.Length; i++)
                {
                    var selectedType = (ValidationResult.ValidationResultType)Enum.GetValues(typeof(ValidationResult.ValidationResultType)).GetValue(i);
                    bool isSelected = _type.HasFlag(selectedType);
                    EditorGUI.BeginChangeCheck();
                    GUILayout.Toggle(isSelected, Styles.tabs[i], EditorStyles.toolbarButton);
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (isSelected)
                        {
                            _type &= ~selectedType;
                        }
                        else
                        {
                            _type |= selectedType;
                        }
                    }
                }
            }
            using (var scope = new EditorGUILayout.ScrollViewScope(_scrollPosition))
            {
                _scrollPosition = scope.scrollPosition;
                ShowList(results);
            }
            GUILayout.FlexibleSpace();
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(hasError ? "Force Export" : "Export"))
                {
                    Exporter.ForceExport(scene);
                    this.Close();
                }
            }
        }

        private void ShowList(List<ListItem> items)
        {
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (!_type.HasFlag(item.validationResult.type))
                {
                    continue;
                }
                item.expanded = EditorGUILayout.BeginFoldoutHeaderGroup(item.expanded, EditorGUIUtility.TrTextContentWithIcon(item.validationResult.name, GetMessageType(item.validationResult.type)));
                if (item.expanded)
                {
                    using (new EditorGUI.IndentLevelScope(2))
                    {
                        EditorGUILayout.LabelField(new GUIContent(item.validationResult.description), EditorStyles.wordWrappedLabel);
                        //EditorGUILayout.Space();
                        if (item.validationResult.resolvers != null)
                        {
                            using (new EditorGUILayout.VerticalScope())
                            {
                                EditorGUILayout.Space();
                                foreach (var resolver in item.validationResult.resolvers)
                                {
                                    using (new EditorGUILayout.HorizontalScope())
                                    {
                                        EditorGUILayout.Space();
                                        if (GUILayout.Button(resolver.text, DefaultStyles.Link))
                                        {
                                            resolver.onResolve?.Invoke();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
            }
        }
        
        private MessageType GetMessageType(ValidationResult.ValidationResultType validationResultType)
        {
            switch (validationResultType)
            {
                case ValidationResult.ValidationResultType.Info:
                    return MessageType.Info;
                case ValidationResult.ValidationResultType.Error:
                    return MessageType.Error;
                case ValidationResult.ValidationResultType.Warning:
                    return MessageType.Warning;
                default:
                    return MessageType.None;
            }
        }
    }
}
