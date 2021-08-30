using System;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk.Editor
{
    [CustomPropertyDrawer(typeof(TreasuredObjectData), true)]
    public class TreasuredObjectDataPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty idProp = property.FindPropertyRelative("_id");
            SerializedProperty descriptionProp = property.FindPropertyRelative("_description");
            SerializedProperty onSelectedProp = property.FindPropertyRelative("_onSelected");

            EditorGUI.BeginProperty(position, label, property);

            if (string.IsNullOrEmpty(idProp.stringValue))
            {
                idProp.stringValue = Guid.NewGuid().ToString();
            }

            EditorGUILayout.PropertyField(idProp);

            EditorGUI.BeginChangeCheck();
            string newName = EditorGUILayout.TextField(new GUIContent("Name"), property.serializedObject.targetObject.name);
            if (EditorGUI.EndChangeCheck() && newName.Length > 0)
            {
                property.serializedObject.targetObject.name = newName;
            }
            EditorGUILayout.PropertyField(descriptionProp);
            EditorGUILayout.PropertyField(onSelectedProp);

            EditorGUI.EndProperty();
        }
    }
}