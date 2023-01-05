using System;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [CustomPropertyDrawer(typeof(ScriptableActionCollection), true)]
    public class ActionCollectionDrawer : PropertyDrawer
    {
        private ActionListDrawer<ScriptableActionCollection> listDrawer;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                EditorGUI.HelpBox(position, $"{property.propertyPath} is not a collection.", MessageType.Error);
                return;
            }
            if (property.serializedObject.isEditingMultipleObjects)
            {
                EditorGUI.HelpBox(position, $"Multi-Editing for {property.propertyPath} is disabled.", MessageType.None);
                return;
            }
            Type type = fieldInfo.FieldType;
            if (property.objectReferenceValue == null && typeof(ScriptableObject).IsAssignableFrom(type))
            {
                property.objectReferenceValue = ScriptableObject.CreateInstance(type);
                property.serializedObject.ApplyModifiedProperties();
            }
            SerializedObject serializedObject = new SerializedObject(property.objectReferenceValue);
            if (listDrawer == null)
            {
                listDrawer = new ActionListDrawer<ScriptableActionCollection>(serializedObject, serializedObject.FindProperty("_actions"), "Actions");
            }
            listDrawer.OnGUI(position);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return listDrawer == null || listDrawer.reorderableList == null ? 0 : listDrawer.reorderableList.GetHeight();
        }
    }
}
