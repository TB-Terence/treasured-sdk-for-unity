using System;
using Treasured.SDK;
using Treasured.UnitySdk;
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
                SerializedProperty idProp = property.FindPropertyRelative("_id");

                if (string.IsNullOrEmpty(idProp.stringValue))
                {
                    idProp.stringValue = Guid.NewGuid().ToString();
                }

                EditorGUI.PropertyField(new Rect(position.x, position.y + 20, position.width, 18), property.FindPropertyRelative("_id"));

                switch (typeProp.stringValue)
                {
                    case "selectObject":
                        SerializedProperty _targetIdProp = property.FindPropertyRelative("_targetId");
                        Rect dropdownRect = EditorGUI.PrefixLabel(new Rect(position.x, position.y + 40, position.width, 18), new GUIContent("Target"));
                        if (EditorGUI.DropdownButton(dropdownRect, new GUIContent(string.IsNullOrEmpty(_targetIdProp.stringValue) ? "Not selected" : _targetIdProp.stringValue), FocusType.Passive))
                        {
                            if (property.serializedObject.targetObject is UnitySdk.TreasuredObject target)
                            {
                                TreasuredMap map = target.GetComponentInParent<TreasuredMap>();
                                if (map != null)
                                {
                                    Hotspot[] hotspots = map.GetComponentsInChildren<Hotspot>();
                                    Interactable[] interactables = map.GetComponentsInChildren<Interactable>();
                                    GenericMenu menu = new GenericMenu();
                                    foreach (var hotspot in hotspots)
                                    {
                                        menu.AddItem(EditorGUIUtility.TrTextContent($"Hotspot/{hotspot.name} | {hotspot.Data.Id}", hotspot.Data.Id), false, () =>
                                        {
                                            _targetIdProp.stringValue = hotspot.Data.Id;
                                            _targetIdProp.serializedObject.ApplyModifiedProperties();
                                        });
                                    }
                                    foreach (var interactable in interactables)
                                    {
                                        menu.AddItem(EditorGUIUtility.TrTextContent($"Interactables/{interactable.name} | {interactable.Data.Id}", interactable.Data.Id), false, () =>
                                        {
                                            _targetIdProp.stringValue = interactable.Data.Id;
                                            _targetIdProp.serializedObject.ApplyModifiedProperties();
                                        });
                                    }
                                    menu.ShowAsContext();
                                }
                            }
                            else
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
                    return 60;
                case "playAudio":
                    return 102;
                case "showText":
                    return 146;
                case "playVideo":
                case "openLink":
                    return 122;
                default:
                    return EditorGUIUtility.singleLineHeight;
            }
        }
    }
}
