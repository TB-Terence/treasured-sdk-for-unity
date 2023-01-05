using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUI.PropertyField(position, property, label);
            }

            EditorGUI.EndProperty();
        }

        
    }
}
