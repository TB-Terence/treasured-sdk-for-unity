using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [CustomPropertyDrawer(typeof(PresetAttribute))]
    public class PresetAttributeDrawer :  PropertyDrawer
    {
        static GUIContent dropdown = EditorGUIUtility.TrIconContent("icon dropdown");

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            if (property.propertyType == SerializedPropertyType.String)
            {
                position.width -= 20;
                EditorGUI.PropertyField(position, property, label);
                position.x += position.width;
                position.width = 20;
                if (EditorGUI.DropdownButton(position, dropdown, FocusType.Passive))
                {
                    ShowPresetMenu(property);
                    EditorGUI.FocusTextInControl(null);
                }
            }
            else
            {
                EditorGUI.PropertyField(position, property, label);
            }
            EditorGUI.EndProperty();
        }

        void ShowPresetMenu(SerializedProperty property)
        {
            PresetAttribute attr = attribute as PresetAttribute;
            GenericMenu menu = new GenericMenu();
            foreach (var item in attr.Values)
            {
                menu.AddItem(new GUIContent(item), false, () =>
                {
                    property.stringValue = item;
                    property.serializedObject.ApplyModifiedProperties();
                });
            }
            menu.ShowAsContext();
            
        }
    }
}
