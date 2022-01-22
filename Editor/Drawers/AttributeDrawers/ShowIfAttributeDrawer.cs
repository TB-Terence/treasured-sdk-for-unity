using UnityEngine;
using UnityEditor;

namespace Treasured.UnitySdk
{
    [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    public class ShowIfAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ShowIfAttribute attribute = (ShowIfAttribute)base.attribute;
            var canEnable = GetShowIfResult(attribute, property);
            var wasEnabled = GUI.enabled;
            GUI.enabled = canEnable;

            if (canEnable)
            {
                EditorGUI.PropertyField(position, property, label, true);
            }

            GUI.enabled = wasEnabled;
        }

        private bool GetShowIfResult(ShowIfAttribute attribute, SerializedProperty property)
        {
            var canEnable = true;

            if (string.IsNullOrWhiteSpace(attribute.ValueToCompare))
                return canEnable;

            var propertyPath = property.propertyPath;
            var conditionPath = propertyPath.Replace(property.name, attribute.ConditionalField);
            var sourcePropertyValue = property.serializedObject.FindProperty(conditionPath);

            if (sourcePropertyValue != null)
            {
                return sourcePropertyValue.stringValue == attribute.ValueToCompare;
            }
            else
            {
                Debug.LogWarning("Attempting to use a ConditionalHideAttribute but no matching SourcePropertyValue found in object: " + attribute.ConditionalField);
            }

            return canEnable;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            ShowIfAttribute condHAtt = (ShowIfAttribute)attribute;
            var enabled = GetShowIfResult(condHAtt, property);

            if (enabled)
            {
                return EditorGUI.GetPropertyHeight(property, label);
            }
            else
            {
                return -EditorGUIUtility.standardVerticalSpacing;
            }
        }
    }
}
