using System;
using System.IO;
using System.Linq;
using Treasured.UnitySdk.Utilities;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [CustomPropertyDrawer(typeof(FloatingIcon))]
    internal class FloatingIconDrawer : PropertyDrawer
    {
        private const string FloatingIconFoldoutKey = "TreasuredSDK_FloatingIconFoldout";

        public static float k_SingleLineHeightWithSpace = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty iconProperty = property.FindPropertyRelative(nameof(FloatingIcon.asset));
            SerializedProperty transformProperty = property.FindPropertyRelative(nameof(FloatingIcon.transform));
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            var expanded = EditorGUI.BeginFoldoutHeaderGroup(new Rect(position.x, position.y, position.width, k_SingleLineHeightWithSpace), SessionState.GetBool(FloatingIconFoldoutKey, true), label);
            if (EditorGUI.EndChangeCheck())
            {
                SessionState.SetBool(FloatingIconFoldoutKey, expanded);
            }
            if (expanded)
            {
                using (new EditorGUI.IndentLevelScope(1))
                {
                    using (var assetChangeScope = new EditorGUI.ChangeCheckScope())
                    {
                        EditorGUI.PropertyField(new Rect(position.x, position.y + k_SingleLineHeightWithSpace, position.width - 20, EditorGUIUtility.singleLineHeight), iconProperty);
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
                                }
                                buttonTransform.SetParent(obj.transform);
                                buttonTransform.localPosition = Vector3.zero;
                                buttonTransform.localRotation = Quaternion.identity;
                                buttonTransform.localScale = Vector3.one;
                                transformProperty.objectReferenceValue = buttonTransform;
                                property.serializedObject.ApplyModifiedProperties();
                            }
                            IconAsset iconAsset = iconProperty.objectReferenceValue as IconAsset;
                            ((transformProperty.objectReferenceValue) as Transform).gameObject.SetIcon(!iconAsset.IsNullOrNone() && !transformProperty.objectReferenceValue.IsNullOrNone() ? iconAsset.icon : null);
                            property.serializedObject.ApplyModifiedProperties();
                        }
                        if (GUI.Button(new Rect(position.xMax - 20, position.y + k_SingleLineHeightWithSpace, 20, EditorGUIUtility.singleLineHeight), GUIIcons.menu, EditorStyles.label))
                        {
                            ShowMenu(property);
                        }
                    }
                    if (!iconProperty.objectReferenceValue.IsNullOrNone())
                    {
                        EditorGUI.PropertyField(new Rect(position.x, position.y + k_SingleLineHeightWithSpace * 2, position.width, EditorGUIUtility.singleLineHeight), transformProperty);
                        if (!transformProperty.objectReferenceValue.IsNullOrNone())
                        {
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
            }
            EditorGUI.EndFoldoutHeaderGroup();
            EditorGUI.EndProperty();
        }


        private void ShowMenu(SerializedProperty property)
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Import Custom Icon"), false , () =>
            {
                EditorGUIUtility.PingObject(FloatingIconProvider.ImportIconAsset());
            });
            menu.AddItem(new GUIContent("Import Custom Icons from Folder"), false, () =>
            {
                string folderPath = EditorUtility.OpenFilePanel("Select Folder", FloatingIconProvider.CustomIconFolderOfCurrentSession, "png");
                if (!string.IsNullOrWhiteSpace(folderPath))
                {
                    FloatingIconProvider.ImportIconAssetsFromFolder(folderPath);
                }
            });
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Set Custom Icon Folder"), false, () =>
            {
                FloatingIconProvider.CustomIconFolder = EditorUtility.OpenFolderPanel("Set Custom Icon Folder", FloatingIconProvider.CustomIconFolder, "");
            });
            menu.AddItem(new GUIContent("Update Icons from Custom Icon Folder"), false, () =>
            {
                FloatingIconProvider.ImportIconAssetsFromFolder(FloatingIconProvider.CustomIconFolder);
            });
            menu.ShowAsContext();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!SessionState.GetBool(FloatingIconFoldoutKey, true))
            {
                return k_SingleLineHeightWithSpace;
            }
            bool assetIsNull = property.FindPropertyRelative(nameof(FloatingIcon.asset)).objectReferenceValue.IsNullOrNone();
            bool transformIsNull = property.FindPropertyRelative(nameof(FloatingIcon.transform)).objectReferenceValue.IsNullOrNone();
            return k_SingleLineHeightWithSpace * (assetIsNull ? 2 : transformIsNull ? 3 : 6);
        }
    }
}
