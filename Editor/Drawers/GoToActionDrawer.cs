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
            if (GUI.Button(new Rect(position.xMax - 25, position.y, 20, EditorGUIUtility.singleLineHeight), EditorGUIUtility.TrIconContent("d_SceneViewCamera", "Preview"), EditorStyles.label))
            {
                if (targetProperty.objectReferenceValue is Hotspot hotspot)
                {
                    SceneView.lastActiveSceneView.LookAt(hotspot.Camera.transform.position, hotspot.Camera.transform.rotation, 0.01f);
                }
            }
            EditorGUILayoutUtils.CreateComponentDropZone<Hotspot>(position, (hotspots) =>
            {
                if (hotspots.Count > 0)
                {
                    var hotspot = hotspots[0];
                    if (hotspot != null)
                    {
                        targetProperty.objectReferenceValue = (Object)hotspot;
                        targetProperty.serializedObject.ApplyModifiedProperties();
                    }
                }
            });
            property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), property.isExpanded, label, true);
            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                EditorGUI.PropertyField(new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing,  position.width, EditorGUIUtility.singleLineHeight), targetProperty);
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
