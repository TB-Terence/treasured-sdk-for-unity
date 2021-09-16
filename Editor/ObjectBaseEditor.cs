using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Treasured.UnitySdk
{
    [CustomEditor(typeof(ObjectBase), true)]
    public class ObjectBaseEditor : Editor
    {
        private SerializedProperty _id;
        //private SerializedProperty transform;
        private SerializedProperty hitbox;
        private SerializedProperty onSelected;

        private ReorderableList reoderableList;

        private List<ActionBase> onSelectedList;
        private Vector2 scrollPosition;

        private void OnEnable()
        {
            onSelectedList = (target as ObjectBase).onSelected;
            _id = serializedObject.FindProperty(nameof(_id));
            //transform = serializedObject.FindProperty(nameof(transform));
            hitbox = serializedObject.FindProperty(nameof(hitbox));
            onSelected = serializedObject.FindProperty(nameof(onSelected));
            reoderableList = new ReorderableList(serializedObject, onSelected)
            {
                drawHeaderCallback = (Rect rect) =>
                {
                    EditorGUI.LabelField(rect, "On Selected");
                    // TODO: Find a way to Collapse all element
                    //if (GUI.Button(new Rect(rect.xMax - 20, rect.y, 20, 20), Styles.menuIcon, EditorStyles.label))
                    //{
                    //    GenericMenu menu = new GenericMenu();
                    //    menu.AddItem(new GUIContent("Collapse All"), false, () =>
                    //    {
                    //        foreach (SerializedProperty element in onSelected)
                    //        {
                    //            element.isExpanded = false;
                    //        }
                    //    });
                    //    menu.ShowAsContext();
                    //}
                },
                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    SerializedProperty element = onSelected.GetArrayElementAtIndex(index);
                    EditorGUI.PropertyField(rect, element, new GUIContent(ObjectNames.NicifyVariableName(onSelectedList[index].Type)), true);
                },
                elementHeightCallback = (int index) =>
                {
                    return EditorGUI.GetPropertyHeight(onSelected.GetArrayElementAtIndex(index));
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
                            SerializedProperty element = onSelected.AppendManagedObject(type);
                            element.isExpanded = true;
                        });
                    }
                    menu.ShowAsContext();
                },
                onRemoveCallback = (ReorderableList list) =>
                {
                    onSelected.RemoveElementAtIndex(list.index);
                }
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            using (var scope = new EditorGUILayout.ScrollViewScope(scrollPosition))
            {
                scrollPosition = scope.scrollPosition;
                using (new EditorGUI.DisabledGroupScope(true))
                {
                    EditorGUILayout.PropertyField(_id);
                }
                EditorGUI.BeginChangeCheck();
                string newName = EditorGUILayout.TextField(new GUIContent("Name"), target.name);
                if (EditorGUI.EndChangeCheck() && !string.IsNullOrEmpty(newName))
                {
                    target.name = newName;
                }
                //EditorGUILayout.PropertyField(transform);
                EditorGUILayout.PropertyField(hitbox);
                reoderableList.DoLayoutList();
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
