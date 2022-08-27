﻿using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Treasured.UnitySdk
{
    [CustomPropertyDrawer(typeof(GoToAction))]
    public class GoToActionDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            SerializedProperty targetProperty = property.FindPropertyRelative(nameof(GoToAction.target));
            SerializedProperty messageProperty = property.FindPropertyRelative(nameof(GoToAction.message));
            EditorGUILayoutUtils.CreateComponentDropZone<Hotspot>(position, (hotspots) =>
            {
                if (hotspots.Count > 0)
                {
                    var hotspot = hotspots[0];
                    if (!hotspot.IsNullOrNone())
                    {
                        targetProperty.objectReferenceValue = hotspot;
                        targetProperty.serializedObject.ApplyModifiedProperties();
                    }
                }
            });
            property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), property.isExpanded, new GUIContent(label.text + (targetProperty.objectReferenceValue.IsNullOrNone() ? " (Not selected)" : $" ({targetProperty.objectReferenceValue.name})")), true);
            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                EditorGUI.PropertyField(new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing,  position.width, EditorGUIUtility.singleLineHeight), targetProperty);
                EditorGUI.PropertyField(new Rect(position.x, position.y + (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 2, position.width, EditorGUIUtility.singleLineHeight), messageProperty);
                EditorGUI.indentLevel--;
            }
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label);
        }
    }
}