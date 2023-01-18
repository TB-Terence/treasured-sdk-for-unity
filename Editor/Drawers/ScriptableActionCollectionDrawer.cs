using System;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [CustomPropertyDrawer(typeof(ScriptableActionCollection), true)]
    public class ScriptableActionCollectionDrawer : PropertyDrawer
    {
        private ActionListDrawer<ScriptableAction> listDrawer;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                EditorGUI.HelpBox(position, $"{property.propertyPath} is not a collection.", MessageType.Error);
                return;
            }
            if (property.objectReferenceValue.IsNullOrNone())
            {
                property.objectReferenceValue = ScriptableObject.CreateInstance(typeof(ScriptableActionCollection));
                property.serializedObject.ApplyModifiedProperties();
            }
            if (listDrawer == null)
            {
                SerializedObject serializedObject = new SerializedObject(property.objectReferenceValue);
                listDrawer = new ActionListDrawer<ScriptableAction>(serializedObject, serializedObject.FindProperty("_actions"), property.displayName);
            }
            listDrawer.OnGUI(position);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return listDrawer == null || listDrawer.reorderableList == null ? 0 : listDrawer.reorderableList.GetHeight();
        }
    }
}
