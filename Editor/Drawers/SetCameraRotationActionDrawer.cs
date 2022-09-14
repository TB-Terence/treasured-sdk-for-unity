using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

namespace Treasured.UnitySdk
{
    [CustomPropertyDrawer(typeof(SetCameraRotationAction))]
    public class SetCameraRotationActionDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), property.isExpanded, label, true);
            if (property.isExpanded)
            {
                SerializedProperty rotationProperty = property.FindPropertyRelative(nameof(SetCameraRotationAction.rotation));
                SerializedProperty speedProperty = property.FindPropertyRelative(nameof(SetCameraRotationAction.speedFactor));
                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, position.height), rotationProperty, new GUIContent("Rotation"));
                if (EditorGUI.EndChangeCheck())
                {
                    SceneView.lastActiveSceneView.LookAtDirect(SceneView.lastActiveSceneView.pivot, rotationProperty.quaternionValue);
                }
                float buttonWidth = position.width / 2;
                if(GUI.Button(new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing, buttonWidth, EditorGUIUtility.singleLineHeight), "Get Current Rotation"))
                {
                    rotationProperty.quaternionValue = SceneView.lastActiveSceneView.camera.transform.rotation;
                }
                if (GUI.Button(new Rect(position.x + buttonWidth, position.y + EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing, buttonWidth, EditorGUIUtility.singleLineHeight), "Set to Rotation"))
                {
                    SceneView.lastActiveSceneView.LookAt(SceneView.lastActiveSceneView.pivot, rotationProperty.quaternionValue);
                }
                speedProperty.floatValue = Mathf.Clamp(EditorGUI.FloatField(new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight * 3 + EditorGUIUtility.standardVerticalSpacing * 2, position.width, EditorGUIUtility.singleLineHeight), speedProperty.displayName, speedProperty.floatValue), 0, 10);
            }
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) + (property.isExpanded ? EditorGUIUtility.singleLineHeight * 3 + EditorGUIUtility.standardVerticalSpacing * 2 : 0);
        }
    }
}
