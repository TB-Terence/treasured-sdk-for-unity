using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Treasured.UnitySdk
{
    internal class ActionGroupListDrawer
    {
        static class Styles
        {
            public static GUIContent toolbarAdd = EditorGUIUtility.TrIconContent("Toolbar Plus", "Create new group");
            public static GUIContent toolbarRemove = EditorGUIUtility.TrIconContent("Toolbar Minus", "Remove selected group");
            public static GUIStyle toolbarButton = new GUIStyle(EditorStyles.label) { fontStyle = FontStyle.Bold };
        }

        private ReorderableList reorderableList;
        private List<ActionBaseListDrawer> groupDrawers = new List<ActionBaseListDrawer>();
        private bool showLabel = false;

        public ActionGroupListDrawer(SerializedObject serializedObject, SerializedProperty elements)
        {
            for (int i = 0; i < elements.arraySize; i++)
            {
                SerializedObject group = new SerializedObject(elements.GetArrayElementAtIndex(i).objectReferenceValue);
                SerializedProperty element = group.FindProperty("_actions");
                groupDrawers.Add(new ActionBaseListDrawer(group, element, $"Group {i + 1}"));
            }
            reorderableList = new ReorderableList(serializedObject, elements)
            {
                footerHeight = 0,
                displayAdd = false,
                displayRemove = false,
                drawHeaderCallback = (Rect rect) =>
                {
                    if (showLabel)
                    {
                        EditorGUI.LabelField(rect, reorderableList.serializedProperty.displayName);
                    }
                    if (GUI.Button(new Rect(rect.xMax - 40, rect.y, 20, rect.height), Styles.toolbarAdd, Styles.toolbarButton))
                    {
                        CreateNewGroup();
                    }
                    using (new EditorGUI.DisabledGroupScope(reorderableList.index == -1))
                    {
                        if (GUI.Button(new Rect(rect.xMax - 20, rect.y, 20, rect.height), Styles.toolbarRemove, Styles.toolbarButton))
                        {
                            RemoveSelectedGroup();
                            EditorGUIUtility.ExitGUI();
                        }
                    }
                },
                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    rect.y += 2; // padding-top
                    groupDrawers[index].reorderableList.serializedProperty.serializedObject.Update();
                    groupDrawers[index].reorderableList.DoList(rect);
                    groupDrawers[index].reorderableList.serializedProperty.serializedObject.ApplyModifiedProperties();
                },
                elementHeightCallback = (int index) =>
                {
                    ReorderableList list = groupDrawers[index].reorderableList;
                    return list.GetHeight() + 2;
                },
                onReorderCallbackWithDetails = (ReorderableList list, int oldIndex, int newIndex) =>
                {
                    groupDrawers[oldIndex].Header = $"Group {newIndex + 1}";
                    groupDrawers[newIndex].Header = $"Group {oldIndex + 1}";
                    var oldGroup = groupDrawers[oldIndex];
                    groupDrawers[oldIndex] = groupDrawers[newIndex];
                    groupDrawers[newIndex] = oldGroup;
                }
            };
        }

        public void OnGUI(bool showLabel = false)
        {
            this.showLabel = showLabel;
            reorderableList.DoLayoutList();
        }

        private void CreateNewGroup()
        {
            reorderableList.serializedProperty.TryAppendScriptableObject(out SerializedProperty newElement, out var group);
            SerializedObject so = new SerializedObject(newElement.objectReferenceValue);
            ActionBaseListDrawer listDrawer = new ActionBaseListDrawer(so, so.FindProperty("_actions"), $"Group {groupDrawers.Count + 1}");
            groupDrawers.Add(listDrawer);
            reorderableList.serializedProperty.serializedObject.ApplyModifiedProperties();
        }

        private void RemoveSelectedGroup()
        {
            int index = reorderableList.index;
            SerializedProperty element = reorderableList.serializedProperty.GetArrayElementAtIndex(index);
            reorderableList.serializedProperty.DeleteArrayElementAtIndex(index);
            if (element != null && element.objectReferenceValue == null)
            {
                reorderableList.serializedProperty.DeleteArrayElementAtIndex(index);
            }
            groupDrawers.RemoveAt(index);
            reorderableList.serializedProperty.serializedObject.ApplyModifiedProperties();
        }
    }
}
