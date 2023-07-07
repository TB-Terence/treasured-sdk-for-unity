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
                if (GUI.Button(new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing, position.width, EditorGUIUtility.singleLineHeight), !RotationRecorder.IsRecording ? "Start Recording" : "Stop Recording"))
                {
                    if (!RotationRecorder.IsRecording)
                    {
                        if(property.serializedObject.targetObject is Hotspot hotspot)
                        {
                            RotationRecorder.Start(hotspot.Camera.transform.position, rotationProperty.quaternionValue, (endRotation) =>
                            {
                                rotationProperty.quaternionValue = endRotation;
                            });
                        }
                        else
                        {
                            TreasuredObject to = property.serializedObject.targetObject as TreasuredObject;
                            if (to)
                            {
                                RotationRecorder.Start(to.transform.position, to.transform.rotation, (endRotation) =>
                                {
                                    rotationProperty.quaternionValue = endRotation;
                                });
                            }
                        }
                    }
                    else
                    {
                        RotationRecorder.Complete();
                    }
                }
                EditorGUI.PropertyField(new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight * 3 + EditorGUIUtility.standardVerticalSpacing * 2, position.width, EditorGUIUtility.singleLineHeight), speedProperty);
            }
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) + (property.isExpanded ? EditorGUIUtility.singleLineHeight * 3 + EditorGUIUtility.standardVerticalSpacing * 2 : 0);
        }
    }
}
