using System.Collections.Generic;
using Treasured.UnitySdk.Validation;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    internal sealed class ValidationWindow : UnityEditor.EditorWindow
    {
        class Styles
        {
            public static GUIContent[] tabs = new GUIContent[] { new GUIContent("All"), EditorGUIUtility.TrIconContent("warning"), EditorGUIUtility.TrIconContent("error") };
        }

        private int _selectedIndex = 0;

        private List<ValidationResult> results = new List<ValidationResult>();
        private List<ValidationResult> warnings = new List<ValidationResult>();
        private List<ValidationResult> errors = new List<ValidationResult>();

        public static void Show(ValidationException e)
        {
            var window = EditorWindow.GetWindow<ValidationWindow>(true, "Validation", true);
            window.results = e.results;
            window.warnings = e.warnings;
            window.errors = e.errors;
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

        private void ShowList(List<ValidationResult> validationResults)
        {
            foreach (var result in validationResults)
            {
                var expanded = EditorGUILayout.BeginFoldoutHeaderGroup(true, EditorGUIUtility.TrTextContentWithIcon(result.name, result.type == ValidationResult.ValidationResultType.Warning ? MessageType.Warning : MessageType.Error));
                if (expanded)
                {
                    using (new EditorGUI.IndentLevelScope(1))
                    {
                        EditorGUILayout.LabelField(new GUIContent(result.description));
                    }
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
            }
        }
    }
}
