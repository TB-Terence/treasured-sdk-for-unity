using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Treasured.UnitySdk
{
    internal class ActionGroupListDrawer
    {
        private ReorderableList reorderableList;
        private List<ActionBaseListDrawer> groupDrawers = new List<ActionBaseListDrawer>();

        public ActionGroupListDrawer(SerializedObject serializedObject, SerializedProperty elements)
        {
            for (int i = 0; i < elements.arraySize; i++)
            {
                SerializedObject group = new SerializedObject(elements.GetArrayElementAtIndex(i).objectReferenceValue);
                SerializedProperty element = group.FindProperty("_actions");
                groupDrawers.Add(new ActionBaseListDrawer(group, element));
            }
            reorderableList = new ReorderableList(serializedObject, elements)
            {
                drawHeaderCallback = (Rect rect) =>
                {
                    EditorGUI.LabelField(rect, "Action Groups");
                },
                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    rect.y += 2;
                    //EditorGUI.LabelField(rect, "Group " + (index + 1));
                    //EditorGUI.PropertyField(rect, reorderableList.serializedProperty.GetArrayElementAtIndex(index));
                    groupDrawers[index].reorderableList.DoList(rect);
                    //SerializedProperty element = elements.GetArrayElementAtIndex(index);
                    //EditorGUI.indentLevel++;
                    //SerializedProperty priorityProp = element.FindPropertyRelative("_priority");
                    //EditorGUI.BeginChangeCheck();
                    //priorityProp.intValue = EditorGUI.IntField(new Rect(rect.xMax - 40, rect.y, 40, 20), priorityProp.intValue);
                    //if (EditorGUI.EndChangeCheck())
                    //{
                    //    SortList(elements);
                    //    GUI.FocusControl(null);
                    //}
                    //EditorGUI.PropertyField(rect, element, new GUIContent(ObjectNames.NicifyVariableName(element.managedReferenceFullTypename.Substring(element.managedReferenceFullTypename.LastIndexOf('.') + 1))), true);
                    //EditorGUI.indentLevel--;
                },
                elementHeightCallback = (int index) =>
                {
                    ReorderableList list = groupDrawers[index].reorderableList;
                    return list.GetHeight() + 2;
                    //return groupDrawers[index].reorderableList.elementHeight;
                    //return EditorGUI.GetPropertyHeight(elements.GetArrayElementAtIndex(index));
                },
                onAddCallback = (ReorderableList list) =>
                {
                    list.serializedProperty.TryAppendScriptableObject(out SerializedProperty newElement, out var group);
                    SerializedObject so = new SerializedObject(newElement.objectReferenceValue);
                    ActionBaseListDrawer listDrawer = new ActionBaseListDrawer(so, so.FindProperty("_actions"));
                    groupDrawers.Add(listDrawer);
                    //list.serializedProperty.arraySize++;
                    //list.serializedProperty.serializedObject.ApplyModifiedProperties();
                    //SerializedProperty groupProp = serializedObject.FindProperty("_actionGroup");
                    //SerializedProperty lastGroup = groupProp.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
                    //SerializedProperty action = groupProp.FindPropertyRelative("_actions");
                    //SerializedProperty action2 = lastGroup.FindPropertyRelative("_actions");
                    //groupDrawers.Add(new ActionBaseListDrawer(serializedObject, action));
                    //SerializedProperty last = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
                    //SerializedProperty actions = last.FindPropertyRelative("_actions");
                    //SerializedProperty a = last.FindPropertyRelative("_a");
                    //groupDrawers.Add(new ActionBaseListDrawer(serializedObject, actions));
                },
                onRemoveCallback = (ReorderableList list) =>
                {
                    int index = list.index;
                    SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);
                    list.serializedProperty.DeleteArrayElementAtIndex(index);
                    if(element != null && element.objectReferenceValue == null)
                    {
                        list.serializedProperty.DeleteArrayElementAtIndex(index);
                    }
                    list.serializedProperty.serializedObject.ApplyModifiedProperties();
                }
            };
        }

        public void OnGUI()
        {
            reorderableList.DoLayoutList();
        }

    }
}
