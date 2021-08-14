using UnityEditor;
using UnityEngine;

namespace Treasured.SDKEditor
{
    [CustomPropertyDrawer(typeof(Treasured.SDK.TreasuredAction))]
    public class TreasuredActionPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            
            SerializedProperty type = property.FindPropertyRelative("_type");
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(new Rect(position.x, position.y, position.width, 18), type);
            if (EditorGUI.EndChangeCheck())
            {
                property.serializedObject.ApplyModifiedProperties();
            }
            if (!string.IsNullOrEmpty(type.stringValue))
            {
                EditorGUI.PropertyField(new Rect(position.x, position.y + 20, position.width, 18), property.FindPropertyRelative("_id"));
            }

            switch (type.stringValue)
            {
                case "selectObject":
                    EditorGUI.PropertyField(new Rect(position.x, position.y + 40, position.width, 18), property.FindPropertyRelative("_targetId"));
                    break;
                case "showText":
                    EditorGUI.PropertyField(new Rect(position.x, position.y + 40, position.width, 80), property.FindPropertyRelative("_content"));
                    break;
                case "openLink":
                case "media/PlayAudio":
                case "media/PlayVideo":
                    EditorGUI.PropertyField(new Rect(position.x, position.y + 40, position.width, 18), property.FindPropertyRelative("_src"));
                    EditorGUI.PropertyField(new Rect(position.x, position.y + 60, position.width, 18), property.FindPropertyRelative("_displayMode"));
                    break;
            }

            EditorGUI.EndProperty();
        }


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty type = property.FindPropertyRelative("_type");
            switch (type.stringValue)
            {
                case "selectObject":
                    return 60;
                case "openLink":
                    return 80;
                case "showText":
                    return 120;
                case "media/PlayAudio":
                case "media/PlayVideo":
                    return 80;
                default:
                    return 20;
            }
        }
    }
}
