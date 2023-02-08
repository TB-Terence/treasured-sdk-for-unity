﻿using System.Collections.Generic;
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
            public static GUIContent[] tabs = new GUIContent[] { new GUIContent("All"), EditorGUIUtility.TrIconContent("warning"), EditorGUIUtility.TrIconContent("error") };
        }

        public class ListItem
        {
            public bool expanded = true;
            public ValidationResult validationResult;
        }

        public TreasuredMap map;
        public List<ListItem> results = new List<ListItem>();
        public List<ListItem> warnings = new List<ListItem>();
        public List<ListItem> errors = new List<ListItem>();

        private int _selectedIndex = 0;
        private Vector2 _scrollPosition;

        public static void Show(TreasuredMap map, ValidationException e)
        {
            var window = EditorWindow.GetWindow<SceneExporterWindow>(true, "Scene Exporter", true);
            window.map = map;
            window.results = e.results.Select(result => new ListItem() { validationResult = result}).ToList();
            window.warnings = window.results.Where(item => item.validationResult.type == ValidationResult.ValidationResultType.Warning).ToList();
            window.errors = window.results.Where(item => item.validationResult.type == ValidationResult.ValidationResultType.Error).ToList();
            Styles.tabs[0].text = $"All({e.results.Count})";
            Styles.tabs[1].text = $"({e.warnings.Count})";
            Styles.tabs[2].text = $"({e.errors.Count})";
            window.Show();
        }

        private void OnGUI()
        {
            DefaultStyles.Init();
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.FlexibleSpace();
                _selectedIndex = GUILayout.SelectionGrid(_selectedIndex, Styles.tabs, Styles.tabs.Length, EditorStyles.toolbarButton);
            }
            using (var scope = new EditorGUILayout.ScrollViewScope(_scrollPosition))
            {
                _scrollPosition = scope.scrollPosition;
                switch (_selectedIndex)
                {
                    case 0:
                        ShowList(results);
                        break;
                    case 1:
                        ShowList(warnings);
                        break;
                    case 2:
                        ShowList(errors);
                        break;
                    default:
                        break;
                }
            }
            GUILayout.FlexibleSpace();
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(errors.Count > 0 ? "Force Export" : "Export"))
                {
                    Exporter.ForceExport(map);
                    this.Close();
                }
            }
        }

        private void ShowList(List<ListItem> items)
        {
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                item.expanded = EditorGUILayout.BeginFoldoutHeaderGroup(item.expanded, EditorGUIUtility.TrTextContentWithIcon(item.validationResult.name, item.validationResult.type == ValidationResult.ValidationResultType.Warning ? MessageType.Warning : MessageType.Error));
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
    }
}