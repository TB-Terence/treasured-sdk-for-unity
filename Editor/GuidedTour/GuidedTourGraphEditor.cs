using System;
using UnityEngine;
using UnityEditor;
using Treasured.UnitySdk.Utilities;
using UnityEditorInternal;
using Newtonsoft.Json;

namespace Treasured.UnitySdk
{
    [CustomEditor(typeof(GuidedTourGraph))]
    public class GuidedTourGraphEditor : Editor 
    {
        private sealed class GuidedTourModalEditorWindow : EditorWindow, IHasCustomMenu
        {
            private static readonly Vector2 WINDOW_SIZE = new Vector2(500, 600);

            private SerializedObject _serializedObject;
            private Vector2 _scrollPosition;

            public static void ShowModal(UnityEngine.Object obj)
            {
                bool isOpened = EditorWindow.HasOpenInstances<GuidedTourModalEditorWindow>();
                var window = EditorWindow.GetWindow<GuidedTourModalEditorWindow>();
                window._serializedObject = new SerializedObject(obj);
                window.titleContent = new GUIContent("Guided Tour Editor");
                var mainWindowPos = EditorGUIUtility.GetMainWindowPosition();
                var windowSize = new Vector2(Math.Min(WINDOW_SIZE.x, mainWindowPos.size.x), Math.Min(WINDOW_SIZE.y, mainWindowPos.size.y));
                if (!isOpened)
                {
                    window.position = new Rect(mainWindowPos.center - windowSize / 2, windowSize);
                }
                window.Show();
            }

            public void AddItemsToMenu(GenericMenu menu)
            {
                if (this._serializedObject.targetObject is GuidedTour gt)
                {
                    menu.AddItem(new GUIContent("Generate and Copy Action Scripts"), false, () =>
                    {
                        string script = JsonConvert.SerializeObject(gt.actionScripts, Formatting.None, JsonExporter.JsonSettings);
                        GUIUtility.systemCopyBuffer = script.Substring(1, script.Length - 2);
                    });
                }
            }

            private void OnGUI()
            {
                if (_serializedObject == null)
                {
                    this.Close();
                    return;
                }
                _serializedObject.Update();
                using(var scope = new EditorGUILayout.ScrollViewScope(_scrollPosition))
                {
                    _scrollPosition = scope.scrollPosition;
                    EditorGUIUtils.DrawPropertiesExcluding(_serializedObject, "m_Script");
                }
                _serializedObject.ApplyModifiedProperties();
            }
        }

        private ReorderableList rl;

        private void OnEnable()
        {
            rl = new ReorderableList(serializedObject, serializedObject.FindProperty(nameof(GuidedTourGraph.tours)));
            rl.headerHeight = 0;
            rl.drawHeaderCallback = (Rect rect) => { };
            rl.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                SerializedProperty element = rl.serializedProperty.GetArrayElementAtIndex(index);
                GuidedTour gt = element.objectReferenceValue as GuidedTour;
                Rect labelRect = new Rect(rect.x, rect.y, rect.width, rect.height);
                EditorGUI.LabelField(rect, new GUIContent(gt.title));
                if (GUI.Button(new Rect(rect.xMax - 20, rect.y, 20, rect.height), EditorGUIUtility.TrIconContent("editicon.sml", "Edit"), EditorStyles.label))
                {
                    GuidedTourModalEditorWindow.ShowModal(gt);
                }
            };
            rl.onAddCallback = (ReorderableList list) =>
            {
                list.serializedProperty.TryAppendScriptableObject(out SerializedProperty elementProperty, out ScriptableObject tour);
            };
            rl.onRemoveCallback = (ReorderableList list) =>
            {
                list.serializedProperty.RemoveElementAtIndex(list.index);
            };
        }
        public override void OnInspectorGUI()
        {
            rl.DoLayoutList();
        }
    }
}
