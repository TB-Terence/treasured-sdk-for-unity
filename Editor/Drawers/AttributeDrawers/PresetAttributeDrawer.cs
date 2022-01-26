using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [CustomPropertyDrawer(typeof(PresetAttribute))]
    public class PresetAttributeDrawer :  PropertyDrawer
    {
        static GUIContent dropdown = EditorGUIUtility.TrIconContent("icon dropdown");
        static GUIContent cloud = EditorGUIUtility.TrIconContent("CloudConnect");

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Type objectType = property.serializedObject.targetObject.GetType();
            FieldInfo fieldInfo = null;
            while (objectType != null) // go up in the hierarchy to find the field
            {
                fieldInfo  = objectType.GetField(property.name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (fieldInfo != null)
                {
                    break;
                }
                objectType = objectType.BaseType;
            }
            OpenUrlAttribute openUrlAttribute = null;
            if (fieldInfo != null)
            {
               openUrlAttribute = fieldInfo.GetCustomAttribute<OpenUrlAttribute>();
            }
            EditorGUI.BeginProperty(position, label, property);
            if (property.propertyType == SerializedPropertyType.String)
            {
                position.width -= 20 + (openUrlAttribute != null ? 20 : 0);
                EditorGUI.PropertyField(position, property, label);
                position.x += position.width;
                position.width = 20;
                if (EditorGUI.DropdownButton(position, dropdown, FocusType.Passive))
                {
                    ShowPresetMenu(property);
                    EditorGUI.FocusTextInControl(null);
                }
                if (openUrlAttribute != null)
                {
                    position.x += 20;
                    if (GUI.Button(position, EditorGUIUtility.TrIconContent("CloudConnect", "Show Icon Previews\n" + openUrlAttribute.Url), GUIStyle.none))
                    {
                        Application.OpenURL(openUrlAttribute.Url);
                    }
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
