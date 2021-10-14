using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Treasured.UnitySdk
{
    internal class ActionBaseListDrawer
    {
        internal ReorderableList reorderableList;

        public ActionBaseListDrawer(SerializedObject serializedObject, SerializedProperty elements)
        {
            reorderableList = new ReorderableList(serializedObject, elements)
            {
                headerHeight = 0,
                drawHeaderCallback = (Rect rect) =>
                {
                    EditorGUI.LabelField(rect, "On Selected");
                },
                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    SerializedProperty element = elements.GetArrayElementAtIndex(index);
                    EditorGUI.indentLevel++;
                    SerializedProperty priorityProp = element.FindPropertyRelative("_priority");
                    EditorGUI.BeginChangeCheck();
                    priorityProp.intValue = EditorGUI.IntField(new Rect(rect.xMax - 40, rect.y, 40, 20), priorityProp.intValue);
                    if (EditorGUI.EndChangeCheck())
                    {
                        SortList(elements);
                        GUI.FocusControl(null);
                    }
                    EditorGUI.PropertyField(rect, element, new GUIContent(ObjectNames.NicifyVariableName(element.managedReferenceFullTypename.Substring(element.managedReferenceFullTypename.LastIndexOf('.') + 1))), true);
                    EditorGUI.indentLevel--;
                },
                elementHeightCallback = (int index) =>
                {
                    return EditorGUI.GetPropertyHeight(elements.GetArrayElementAtIndex(index));
                },
                onAddDropdownCallback = (Rect buttonRect, ReorderableList list) =>
                {
                    var actionTypes = TypeCache.GetTypesDerivedFrom<ActionBase>().Where(x => !x.IsAbstract);
                    GenericMenu menu = new GenericMenu();
                    foreach (var type in actionTypes)
                    {
                        string name = ObjectNames.NicifyVariableName(type.Name);
                        CategoryAttribute attribute = (CategoryAttribute)Attribute.GetCustomAttribute(type, typeof(CategoryAttribute));
                        menu.AddItem(new GUIContent(attribute != null ? $"{attribute.Path}/{name}" : name), false, () =>
                        {
                            SerializedProperty element = elements.AppendManagedObject(type);
                            element.isExpanded = false;
                        });
                    }
                    menu.ShowAsContext();
                },
                onRemoveCallback = (ReorderableList list) =>
                {
                    elements.RemoveElementAtIndex(list.index);
                }
            };
        }

        public void OnGUI()
        {
            reorderableList.DoLayoutList();
        }

        private void SortList(SerializedProperty elements)
        {
            SerializedProperty[] array = new SerializedProperty[elements.arraySize];
            Dictionary<SerializedProperty, int> order = new Dictionary<SerializedProperty, int>();
            for (int i = 0; i < elements.arraySize; i++)
            {
                array[i] = elements.GetArrayElementAtIndex(i);
            }
            SerializedProperty[] sortedArray = array.OrderBy((element) =>
            {
                return element.FindPropertyRelative("_priority").intValue;
            }).ToArray();
            for (int i = 0; i < sortedArray.Length; i++)
            {
                order[sortedArray[i]] = i;
            } 
            for (int i = 0; i < elements.arraySize; i++)
            {
                elements.MoveArrayElement(order[array[i]], i);
            }
            elements.serializedObject.ApplyModifiedProperties();
        }
    }
}
