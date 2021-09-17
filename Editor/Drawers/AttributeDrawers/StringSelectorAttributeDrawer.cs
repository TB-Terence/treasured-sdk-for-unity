using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk.Editor
{
    [CustomPropertyDrawer(typeof(StringSelectorAttribute))]
    public class StringSelectorAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.HelpBox(position, $"StringSelectorAttribute only works on string field.", MessageType.Error);
                return;
            }
            StringSelectorAttribute attr = attribute as StringSelectorAttribute;
            if (attr.Values == null)
            {
                EditorGUI.HelpBox(position, $"No value to select.", MessageType.Error);
                return;
            }
            Rect buttonRect = EditorGUI.PrefixLabel(position, label);
            if (EditorGUI.DropdownButton(buttonRect, new GUIContent(ObjectNames.NicifyVariableName(property.stringValue)), FocusType.Passive))
            {
                GenericMenu menu = new GenericMenu();
                foreach (var value in attr.Values)
                {
                    menu.AddItem(new GUIContent(ObjectNames.NicifyVariableName(value)), false, () =>
                    {
                        property.stringValue = value;
                        property.serializedObject.ApplyModifiedProperties();
                        property.serializedObject.Update();
                    });
                }
                menu.DropDown(buttonRect);
            }
            EditorGUI.EndProperty();
        }
    }
}
