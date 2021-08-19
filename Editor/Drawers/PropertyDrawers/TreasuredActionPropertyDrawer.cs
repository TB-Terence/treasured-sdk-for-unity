using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Treasured.SDKEditor
{
    [CustomPropertyDrawer(typeof(Treasured.SDK.TreasuredAction))]
    public class TreasuredActionPropertyDrawer : PropertyDrawer
    {
        private static HashSet<string> ids = new HashSet<string>();

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
            SerializedProperty idProp = property.FindPropertyRelative("_id");

            if (string.IsNullOrEmpty(idProp.stringValue))
            {
                idProp.stringValue = Guid.NewGuid().ToString();
            }

            if (!string.IsNullOrEmpty(type.stringValue)) // if type selected
            {
                using (new EditorGUI.DisabledGroupScope(true))
                {
                    EditorGUI.PropertyField(new Rect(position.x, position.y + 20, position.width - 22, 18), property.FindPropertyRelative("_id"));
                }
                if (GUI.Button(new Rect(new Rect(position.xMax - 20, position.y + 20, 20, 18)), EditorGUIUtility.TrIconContent("Refresh", "Regenerate ID")))
                {
                    idProp.stringValue = Guid.NewGuid().ToString();
                }
            }

            switch (type.stringValue)
            {
                case "selectObject":
                    SerializedProperty _targetIdProp = property.FindPropertyRelative("_targetId");
                    if (string.IsNullOrEmpty(_targetIdProp.stringValue) || !TreasuredDataEditorWindow.ObjectIds.TryGetValue(_targetIdProp.stringValue, out string path))
                    {
                        path = "Not Selected";
                    }
                    Rect dropdownRect = EditorGUI.PrefixLabel(new Rect(position.x, position.y + 40, position.width, 18), new GUIContent("Target ID"));
                    if (EditorGUI.DropdownButton(dropdownRect, new GUIContent(text: path), FocusType.Passive))
                    {
                        GenericMenu menu = new GenericMenu();
                        foreach (var idInfo in TreasuredDataEditorWindow.ObjectIds)
                        {
                            if (_targetIdProp.stringValue.Equals(idInfo.Key))
                            {
                                continue;
                            }
                            menu.AddItem(new GUIContent(idInfo.Value), false, () =>
                            {
                                _targetIdProp.stringValue = idInfo.Key;
                                _targetIdProp.serializedObject.ApplyModifiedProperties();
                            });
                        }
                        menu.ShowAsContext();
                    }
                    break;
                case "showText":
                    SerializedProperty contentProp = property.FindPropertyRelative("_content");
                    float contentPropHeight = EditorGUI.GetPropertyHeight(contentProp);
                    EditorGUI.PropertyField(new Rect(position.x, position.y + 38, position.width, contentPropHeight), property.FindPropertyRelative("_content"));
                    break;
                case "openLink":
                case "media/PlayAudio":
                case "media/PlayVideo":
                    SerializedProperty srcProp = property.FindPropertyRelative("_src");
                    float srcPropHeight = EditorGUI.GetPropertyHeight(srcProp);
                    EditorGUI.PropertyField(new Rect(position.x, position.y + 38, position.width, srcPropHeight), srcProp);
                    EditorGUI.PropertyField(new Rect(position.x, position.y + 40 + srcPropHeight, position.width, 18), property.FindPropertyRelative("_displayMode"));
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
                case "showText":
                    return 102;
                case "media/PlayAudio":
                case "media/PlayVideo":
                case "openLink":
                    return 122;
                default:
                    return 20;
            }
        }
    }
}
