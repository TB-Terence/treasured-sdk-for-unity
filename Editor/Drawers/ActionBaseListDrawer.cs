using System;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Treasured.UnitySdk
{
    internal class ActionBaseListDrawer
    {
        internal ReorderableList reorderableList;
        public string Header { get; set; }

        public ActionBaseListDrawer(SerializedObject serializedObject, SerializedProperty elements, string header)
        {
            Header = header;
            reorderableList = new ReorderableList(serializedObject, elements)
            {
                drawHeaderCallback = (Rect rect) =>
                {
                    EditorGUI.LabelField(rect, Header);
                },
                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    SerializedProperty element = elements.GetArrayElementAtIndex(index);
                    EditorGUI.indentLevel++;
                    EditorGUI.PropertyField(rect, element, new GUIContent(ObjectNames.NicifyVariableName(element.managedReferenceFullTypename.Substring(element.managedReferenceFullTypename.LastIndexOf('.') + 1))), true);
                    EditorGUI.indentLevel--;
                },
                elementHeightCallback = (int index) =>
                {
                    return EditorGUI.GetPropertyHeight(elements.GetArrayElementAtIndex(index));
                },
                onAddDropdownCallback = (Rect buttonRect, ReorderableList list) =>
                {
                    var actionTypes = TypeCache.GetTypesDerivedFrom<Action>().Where(x => !x.IsAbstract);
                    GenericMenu menu = new GenericMenu();
                    foreach (var type in actionTypes)
                    {
                        string name = ObjectNames.NicifyVariableName(type.Name);
                        CategoryAttribute attribute = (CategoryAttribute)Attribute.GetCustomAttribute(type, typeof(CategoryAttribute));
                        menu.AddItem(new GUIContent(attribute != null ? $"{attribute.Path}/{name}" : name), false, () =>
                        {
                            SerializedProperty element = elements.AppendManagedObject(type);
                            element.isExpanded = false;
                            element.serializedObject.ApplyModifiedProperties();
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
            reorderableList.serializedProperty.serializedObject.Update();
            reorderableList.DoLayoutList();
            reorderableList.serializedProperty.serializedObject.ApplyModifiedProperties();
        }
    }
}
