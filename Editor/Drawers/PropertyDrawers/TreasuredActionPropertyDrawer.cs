using System;
using Treasured.SDK;
using UnityEditor;
using UnityEngine;

namespace Treasured.SDKEditor
{
    [CustomPropertyDrawer(typeof(TreasuredAction))]
    public class TreasuredActionPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            SerializedProperty typeProp = property.FindPropertyRelative("_type");
            SerializedProperty srcProp = property.FindPropertyRelative("_src");

            property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, 20), property.isExpanded, new GUIContent(string.IsNullOrEmpty(typeProp.stringValue) ? "Type not selected" : ObjectNames.NicifyVariableName(typeProp.stringValue)), true);
            float srcPropHeight = EditorGUI.GetPropertyHeight(srcProp);
            if (property.isExpanded)
            {
                position.y += 20;
                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(new Rect(position.x, position.y, position.width, 18), typeProp);
                if (EditorGUI.EndChangeCheck())
                {
                    property.serializedObject.ApplyModifiedProperties();
                }
                SerializedProperty idProp = property.FindPropertyRelative("_id");

                if (string.IsNullOrEmpty(idProp.stringValue))
                {
                    idProp.stringValue = Guid.NewGuid().ToString();
                }

                if (!string.IsNullOrEmpty(typeProp.stringValue)) // if type selected
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

                switch (typeProp.stringValue)
                {
                    case "selectObject":
                        SerializedProperty _targetIdProp = property.FindPropertyRelative("_targetId");
                        if (string.IsNullOrEmpty(_targetIdProp.stringValue) || !TreasuredDataEditor.ObjectIds.TryGetValue(_targetIdProp.stringValue, out string path))
                        {
                            path = "Not Selected";
                        }
                        Rect dropdownRect = EditorGUI.PrefixLabel(new Rect(position.x, position.y + 40, position.width, 18), new GUIContent("Target ID"));
                        if (EditorGUI.DropdownButton(dropdownRect, new GUIContent(text: path), FocusType.Passive))
                        {
                            string idPath = property.propertyPath;
                            int index = IndexOf(property.propertyPath, '.', 3);
                            if (index != -1)
                            {
                                idPath = $"{property.propertyPath.Substring(0, index)}._id";
                                SerializedProperty id = property.serializedObject.FindProperty(idPath);
                                GenericMenu menu = new GenericMenu();
                                foreach (var idInfo in TreasuredDataEditor.ObjectIds)
                                {
                                    if (_targetIdProp.stringValue.Equals(idInfo.Key) || idInfo.Key.Equals(id.stringValue))
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
                        }
                        break;
                    case "showText":
                        SerializedProperty contentProp = property.FindPropertyRelative("_content");
                        float contentPropHeight = EditorGUI.GetPropertyHeight(contentProp);
                        EditorGUI.PropertyField(new Rect(position.x, position.y + 38, position.width, contentPropHeight), property.FindPropertyRelative("_content"));
                        EditorGUI.PropertyField(new Rect(position.x, position.y + 40 + contentPropHeight, position.width, 18), property.FindPropertyRelative("_style"));
                        break;
                    case "openLink":
                    case "playVideo":
                        EditorGUI.PropertyField(new Rect(position.x, position.y + 38, position.width, srcPropHeight), srcProp);
                        EditorGUI.PropertyField(new Rect(position.x, position.y + 40 + srcPropHeight, position.width, 18), property.FindPropertyRelative("_displayMode"));
                        break;
                    case "playAudio":
                        EditorGUI.PropertyField(new Rect(position.x, position.y + 38, position.width, srcPropHeight), srcProp);
                        break;
                }
            }
            EditorGUI.EndProperty();
        }

        private int IndexOf(string value, char c, int count)
        {
            if (string.IsNullOrEmpty(value) || count < 1)
            {
                return -1;
            }
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i].Equals(c))
                {
                    count--;
                    if (count == 0)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded)
            {
                return 20;
            }
            SerializedProperty type = property.FindPropertyRelative("_type");
            switch (type.stringValue)
            {
                case "selectObject":
                    return 80;
                case "playAudio":
                    return 122;
                case "showText":
                    return 146;
                case "playVideo":
                case "openLink":
                    return 142;
                default:
                    return 40;
            }
        }
    }
}
