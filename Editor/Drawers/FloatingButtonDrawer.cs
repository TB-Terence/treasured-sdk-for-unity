using System;
using System.IO;
using System.Linq;
using Treasured.UnitySdk.Utilities;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [CustomPropertyDrawer(typeof(FloatingButton))]
    internal class FloatingButtonDrawer : PropertyDrawer
    {
        public static float k_SingleLineHeightWithSpace = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty iconProperty = property.FindPropertyRelative(nameof(FloatingButton.icon));
            SerializedProperty transformProperty = property.FindPropertyRelative(nameof(FloatingButton.transform));
            EditorGUI.BeginProperty(position, label, property);
            property.isExpanded = EditorGUI.BeginFoldoutHeaderGroup(new Rect(position.x, position.y, position.width, k_SingleLineHeightWithSpace), property.isExpanded, label);
            if (property.isExpanded)
            {
                using (new EditorGUI.IndentLevelScope(1))
                {
                    using (var assetChangeScope = new EditorGUI.ChangeCheckScope())
                    {
                        EditorGUI.PropertyField(new Rect(position.x, position.y + k_SingleLineHeightWithSpace, position.width, EditorGUIUtility.singleLineHeight), iconProperty);
                        if (assetChangeScope.changed)
                        {
                            var obj = property.serializedObject.targetObject as MonoBehaviour;
                            if (transformProperty.objectReferenceValue.IsNullOrNone())
                            {
                                var buttonTransform = obj.transform.Find("Button");
                                if (buttonTransform == null)
                                {
                                    GameObject go = new GameObject("Button");
                                    buttonTransform = go.transform;
                                    go.transform.SetParent(obj.transform);
                                    go.transform.position = Vector3.zero;
                                }
                                transformProperty.objectReferenceValue = buttonTransform;
                                property.serializedObject.ApplyModifiedProperties();
                            }
                            IconAsset iconAsset = iconProperty.objectReferenceValue as IconAsset;
                            if (!iconAsset.IsNullOrNone() && !transformProperty.objectReferenceValue.IsNullOrNone())
                            {
                                ((transformProperty.objectReferenceValue) as Transform).gameObject.SetIcon(iconAsset.icon);
                            }
                            property.serializedObject.ApplyModifiedProperties();
                        }
                    }
                    if (transformProperty.objectReferenceValue.IsNullOrNone())
                    {
                        EditorGUI.PropertyField(new Rect(position.x, position.y + k_SingleLineHeightWithSpace * 2, position.width - 60, EditorGUIUtility.singleLineHeight), transformProperty);
                        if (GUI.Button(new Rect(position.xMax - 58, position.y + k_SingleLineHeightWithSpace * 2, 58, EditorGUIUtility.singleLineHeight), new GUIContent("Create")))
                        {
                            Component component = transformProperty.serializedObject.targetObject as Component;
                            if (component)
                            {
                                GameObject go = new GameObject(ObjectNames.NicifyVariableName(property.name));
                                go.transform.parent = component.transform;
                                go.transform.localPosition = Vector3.zero;
                                go.transform.localRotation = Quaternion.identity;
                                go.transform.localScale = Vector3.one;
                                transformProperty.objectReferenceValue = go.transform;
                                //if (EditorGUIUtils.DefaultButtonIconTexture)
                                //{
                                //    go.SetIcon(EditorGUIUtils.DefaultButtonIconTexture);
                                //    iconProperty.stringValue = EditorGUIUtils.DefaultButtonIconTexture.name;
                                //}
                            }
                        }
                    }
                    else
                    {
                        EditorGUI.PropertyField(new Rect(position.x, position.y + k_SingleLineHeightWithSpace * 2, position.width, EditorGUIUtility.singleLineHeight), transformProperty);
                        using (new EditorGUI.IndentLevelScope(1))
                        {
                            using (var scope = new EditorGUI.ChangeCheckScope())
                            {
                                Transform transform = transformProperty.objectReferenceValue as Transform;
                                transform.localPosition = EditorGUI.Vector3Field(new Rect(position.x, position.y + k_SingleLineHeightWithSpace * 3, position.width, EditorGUIUtility.singleLineHeight), "Position", transform.localPosition);
                                transform.localEulerAngles = EditorGUI.Vector3Field(new Rect(position.x, position.y + k_SingleLineHeightWithSpace * 4, position.width, EditorGUIUtility.singleLineHeight), "Rotation", transform.localEulerAngles);
                                transform.localScale = EditorGUI.Vector3Field(new Rect(position.x, position.y + k_SingleLineHeightWithSpace * 5, position.width, EditorGUIUtility.singleLineHeight), "Size", transform.localScale);
                                if (scope.changed)
                                {
                                    transformProperty.serializedObject.ApplyModifiedProperties();
                                }
                            }
                        }
                    }
                }
            }
            EditorGUI.EndFoldoutHeaderGroup();
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded)
            {
                return k_SingleLineHeightWithSpace;
            }
            return k_SingleLineHeightWithSpace * (property.FindPropertyRelative(nameof(FloatingButton.transform)).objectReferenceValue == null ? 3 : 6);
        }
    }
}
