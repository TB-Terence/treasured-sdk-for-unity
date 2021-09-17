using System;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk.Editor
{
    [CustomPropertyDrawer(typeof(UniqueIdAttribute))]
    public class UniqueIdAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            if (property.propertyType == SerializedPropertyType.String)
            {
                using (new EditorGUI.DisabledGroupScope(true))
                {
                    EditorGUI.TextField(new Rect(position.x, position.y, position.width - 22, position.height), label, property.stringValue);
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
