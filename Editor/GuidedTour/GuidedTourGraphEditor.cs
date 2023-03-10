using System;
using UnityEngine;
using UnityEditor;
using Treasured.UnitySdk.Utilities;
using UnityEditorInternal;
using Newtonsoft.Json;
using System.Linq;

namespace Treasured.UnitySdk
{
    [CustomEditor(typeof(GuidedTourGraph))]
    internal class GuidedTourGraphEditor : Editor 
    {
        public static GuidedTourGraph Current { get; private set; }
        public TreasuredMap Map { get; set; }
        public sealed class GuidedTourModalEditorWindow : EditorWindow
        {
            private static readonly Vector2 WINDOW_SIZE = new Vector2(500, 600);

            public SerializedObject serializedObject;
            private Vector2 _scrollPosition;

            public static void ShowModal(UnityEngine.Object obj)
            {
                bool isOpened = EditorWindow.HasOpenInstances<GuidedTourModalEditorWindow>();
                var window = EditorWindow.GetWindow<GuidedTourModalEditorWindow>();
                window.serializedObject = new SerializedObject(obj);
                window.titleContent = new GUIContent("Guided Tour Editor");
                var mainWindowPos = EditorGUIUtility.GetMainWindowPosition();
                var windowSize = new Vector2(Math.Min(WINDOW_SIZE.x, mainWindowPos.size.x), Math.Min(WINDOW_SIZE.y, mainWindowPos.size.y));
                if (!isOpened)
                {
                    window.position = new Rect(mainWindowPos.center - windowSize / 2, windowSize);
                }
                window.Show();
            }

            private void OnGUI()
            {
                if (serializedObject == null)
                {
                    this.Close();
                    return;
                }
                using (var scope = new EditorGUILayout.ScrollViewScope(_scrollPosition))
                {
                    _scrollPosition = scope.scrollPosition;
                    EditorGUIUtils.DrawPropertiesExcluding(serializedObject, "m_Script");
                }
            }
        }

        private ReorderableList rl;

        private void OnEnable()
        {
            Current = target as GuidedTourGraph;
            rl = new ReorderableList(serializedObject, serializedObject.FindProperty(nameof(GuidedTourGraph.tours)));
            rl.drawHeaderCallback = (Rect rect) =>
            {
                using (new EditorGUI.DisabledGroupScope(rl.serializedProperty.arraySize == 0))
                {
                    if (GUI.Button(new Rect(rect.xMax - 40, rect.y, 40, rect.height), new GUIContent("Clear", "Remove all tours"), EditorStyles.boldLabel))
                    {
                        if (EditorUtility.DisplayDialog("Warning", "You are about to remove all tours. This cannot be undone.", "Confirm", "Cancel"))
                        {
                            rl.serializedProperty.ClearArray();
                            rl.serializedProperty.serializedObject.ApplyModifiedProperties();
                            rl.DoLayoutList(); // hacky way of resolving array index out of bounds error after clear.
                        }
                    }
                }
            };
            rl.onSelectCallback = (ReorderableList list) =>
            {
                SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(list.index);
                GuidedTourModalEditorWindow.ShowModal(element.objectReferenceValue);
            };
            rl.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                SerializedProperty element = rl.serializedProperty.GetArrayElementAtIndex(index);
                GuidedTour gt = element.objectReferenceValue as GuidedTour;
                Rect labelRect = new Rect(rect.x, rect.y, rect.width, rect.height);
                EditorGUI.LabelField(rect, new GUIContent(gt.title), EditorStyles.boldLabel);
            };
            rl.onAddDropdownCallback = (Rect buttonRect, ReorderableList list) =>
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("New"), false, () =>
                {
                    CreateNew(list, "New Tour");
                });
                menu.AddItem(new GUIContent("Quick Tour/Navigate Through Hotspots"), false, () =>
                {
                    GuidedTour tour = CreateNew(list, "Quick Tour");
                    tour.actionScripts = new ScriptableActionCollection();
                    foreach (var hotspot in Map.Hotspots)
                    {
                        tour.actionScripts.Add(new GoToAction()
                        {
                            target = hotspot
                        });
                        tour.actionScripts.Add(new SetCameraRotationAction()
                        {
                            rotation = hotspot.Camera.transform.rotation
                        });
                        if (!hotspot.actionGraph.TryGetActionGroup("onSelect", out var onSelect)) continue;
                        foreach (var action in onSelect)
                        {
                            tour.actionScripts.Add(action);
                        }
                    }
                    serializedObject.Update();
                });
                menu.ShowAsContext();
            };
            rl.onRemoveCallback = (ReorderableList list) =>
            {
                list.serializedProperty.RemoveElementAtIndex(list.index);
            };
        }

        private GuidedTour CreateNew(ReorderableList list, string defaultName = "")
        {
            list.serializedProperty.TryAppendScriptableObject(out SerializedProperty elementProperty, out ScriptableObject tour);
            GuidedTour guidedTour = tour as GuidedTour;
            if (list.count == 1)
            {
                guidedTour.name = guidedTour.title = "default";
            }
            else
            {
                string[] tourNames = (serializedObject.targetObject as GuidedTourGraph)?.tours.Select(tour => tour.name).ToArray();
                guidedTour.title = guidedTour.name = ObjectNames.GetUniqueName(tourNames, defaultName);
            }
            elementProperty.serializedObject.Update();
            elementProperty.serializedObject.ApplyModifiedProperties();
            GuidedTourModalEditorWindow.ShowModal(tour);
            list.index = list.count - 1;
            return tour as GuidedTour;
        }

        public override void OnInspectorGUI()
        {
            rl.DoLayoutList();
            if(!Current.IsNullOrNone() && !Current.tours.Any(t => t.title.Equals("default")))
            {
                EditorGUILayout.HelpBox("A scene uses guided tour must have a tour named 'default'.", MessageType.Error);
            }
        }
    }
}
