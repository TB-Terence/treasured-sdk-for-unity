using System.Collections;
using System.Collections.Generic;
using Treasured.UnitySdk.Utilities;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [CustomPropertyDrawer(typeof(EnableIfAttribute))]
    public class EnableIfAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attribute = (EnableIfAttribute)base.attribute;
            var canEnable = CompareValues(attribute, property);
            using (new EditorGUI.DisabledScope(!canEnable))
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }

        private bool CompareValues(EnableIfAttribute attribute, SerializedProperty property)
        {
            var canEnable = false;

            var propertyPath = property.propertyPath;
            var conditionPath = propertyPath.Replace(property.name, attribute.ConditionalField);
            var sourcePropertyValue = property.serializedObject.FindProperty(conditionPath);
            if (sourcePropertyValue.objectReferenceValue == null)
            {
                canEnable = true;
            }

            return canEnable;
        }
    }
}
