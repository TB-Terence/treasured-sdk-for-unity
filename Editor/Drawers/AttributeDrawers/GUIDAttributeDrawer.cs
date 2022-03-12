using System;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [CustomPropertyDrawer(typeof(GUIDAttribute))]
    public class GUIDAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            if (property.propertyType == SerializedPropertyType.String)
            {
                using (new EditorGUI.DisabledGroupScope(true))
                {
                    EditorGUI.PropertyField(new Rect(position.x, position.y, position.width - 22, position.height), property, label);
                }
                if (GUI.Button(new Rect(position.xMax - 20, position.y, 20, position.height), EditorGUIUtility.TrIconContent("Refresh", "Regenerate Id")))
                {
                    property.stringValue = Guid.NewGuid().ToString();
                }
            }
            else
            {
                EditorGUI.PropertyField(position, property, label);
            }
            EditorGUI.EndProperty();
        }
    }
}
