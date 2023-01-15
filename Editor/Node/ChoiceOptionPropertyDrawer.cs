using UnityEditor;
using UnityEngine;
using static Treasured.Actions.ChoiceNode;

namespace Treasured.UnitySdk
{
    [CustomPropertyDrawer(typeof(ChoiceOptionAttribute))]
    public class ChoiceOptionPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.PropertyField(position, property, GUIContent.none);
            EditorGUI.EndProperty();
        }
    }
}
