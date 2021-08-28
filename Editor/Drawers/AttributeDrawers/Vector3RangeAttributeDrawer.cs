using UnityEngine;
using UnityEditor;
using Treasured.SDK;

namespace Treasured.SDKEditor
{
    [CustomPropertyDrawer(typeof(Vector3RangeAttribute))]
    public class Vector3RangeAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            if (property.propertyType != SerializedPropertyType.Vector3 || property.propertyType != SerializedPropertyType.Vector3)
            {
                EditorGUI.HelpBox(position, "Vector3RangeAttribute only works on Vector3 field", MessageType.Error);
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(position, property, label);
                if (EditorGUI.EndChangeCheck())
                {
                    Validate(property);
                }
            }
            EditorGUI.EndProperty();
        }

        void Validate(SerializedProperty property)
        {
            Vector3RangeAttribute attr = attribute as Vector3RangeAttribute;
            switch (property.propertyType)
            {
                case SerializedPropertyType.Vector3:
                    property.vector3Value = new Vector3()
                    {

                        x = Mathf.Clamp(property.vector3Value.x, attr.MinX, attr.MaxX),
                        y = Mathf.Clamp(property.vector3Value.y, attr.MinY, attr.MaxY),
                        z = Mathf.Clamp(property.vector3Value.z, attr.MinZ, attr.MaxZ)
                    };
                    break;
                case SerializedPropertyType.Vector3Int:
                    property.vector3IntValue = new Vector3Int()
                    {
                        x = Mathf.Clamp(property.vector3IntValue.x, (int)attr.MinX, (int)attr.MaxX),
                        y = Mathf.Clamp(property.vector3IntValue.y, (int)attr.MinY, (int)attr.MaxY),
                        z = Mathf.Clamp(property.vector3IntValue.z, (int)attr.MinZ, (int)attr.MaxZ)
                    };
                    break;
            }
        }
    }
}
