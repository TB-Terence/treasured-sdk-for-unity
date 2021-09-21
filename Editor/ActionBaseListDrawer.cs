using System;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Treasured.UnitySdk
{
    public class ActionBaseListDrawer
    {
        private ReorderableList reorderableList;

        public ActionBaseListDrawer(SerializedObject serializedObject, SerializedProperty elements)
        {
            reorderableList = new ReorderableList(serializedObject, elements)
            {
                drawHeaderCallback = (Rect rect) =>
                {
                    EditorGUI.LabelField(rect, "On Selected");
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
                    var actionTypes = TypeCache.GetTypesDerivedFrom<ActionBase>().Where(x => !x.IsAbstract);
                    GenericMenu menu = new GenericMenu();
                    foreach (var type in actionTypes)
                    {
                        string name = ObjectNames.NicifyVariableName(type.Name);
                        CategoryAttribute attribute = (CategoryAttribute)Attribute.GetCustomAttribute(type, typeof(CategoryAttribute));
                        menu.AddItem(new GUIContent(attribute != null ? $"{attribute.Path}/{name}" : name), false, () =>
                        {
                            SerializedProperty element = elements.AppendManagedObject(type);
                            element.isExpanded = true;
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
    }
}
