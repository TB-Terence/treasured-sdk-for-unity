using System;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Treasured.UnitySdk
{
    internal class ActionListDrawer<T> where T : class
    {
        internal ReorderableList reorderableList;
        public string Header { get; set; }

        public ActionListDrawer(SerializedObject serializedObject, SerializedProperty elements, string header)
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
                    using (new EditorGUI.IndentLevelScope(1))
                    {
                        string name = element.managedReferenceFullTypename.Substring(element.managedReferenceFullTypename.LastIndexOf('.') + 1);
                        if (name.EndsWith("Action") && name.Length > 6)
                        {
                            name = name.Substring(0, name.Length - 6);
                        }
                        if (name.Length > 1)
                        {
                            name = char.ToLower(name[0]) + name.Substring(1);
                        }
                        EditorGUI.PropertyField(rect, element, new GUIContent(ObjectNames.NicifyVariableName(name)), true);
                    }
                },
                elementHeightCallback = (int index) =>
                {
                    return EditorGUI.GetPropertyHeight(elements.GetArrayElementAtIndex(index));
                },
                onAddDropdownCallback = (Rect buttonRect, ReorderableList list) =>
                {
                    var actionTypes = TypeCache.GetTypesDerivedFrom<T>().Where(x => !x.IsAbstract && !x.IsDefined(typeof(ObsoleteAttribute), true));
                    GenericMenu menu = new GenericMenu();
                    foreach (var type in actionTypes)
                    {
                        CategoryAttribute attribute = (CategoryAttribute)Attribute.GetCustomAttribute(type, typeof(CategoryAttribute));
                        string name = type.Name;
                        if (name.EndsWith("Action") && name.Length > 6)
                        {
                            name = name.Substring(0, name.Length - 6);
                        }
                        name = ObjectNames.NicifyVariableName(name);
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

        public void OnGUI(Rect rect)
        {
            reorderableList.serializedProperty.serializedObject.Update();
            reorderableList.DoList(rect);
            reorderableList.serializedProperty.serializedObject.ApplyModifiedProperties();
        }

        public void OnGUILayout()
        {
            reorderableList.serializedProperty.serializedObject.Update();
            reorderableList.DoLayoutList();
            reorderableList.serializedProperty.serializedObject.ApplyModifiedProperties();
        }

    }
}
