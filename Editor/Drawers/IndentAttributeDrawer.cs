using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [CustomPropertyDrawer(typeof(IndentAttribute))]
    public class IndentAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            IndentAttribute attr = attribute as IndentAttribute;
            EditorGUI.indentLevel += attr.IndentLevel;
            EditorGUI.PropertyField(position, property, label);
            EditorGUI.indentLevel -= attr.IndentLevel; ;
            EditorGUI.EndProperty();
        }
    }
}
