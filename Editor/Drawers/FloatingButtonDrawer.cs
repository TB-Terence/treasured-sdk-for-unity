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

        public void SetIcon(SerializedProperty property, string name)
        {
            SerializedProperty transformProperty = property.FindPropertyRelative(nameof(FloatingButton.transform));
            if (EditorGUIUtils.TryGetButtonIconTexture(name, out var texture))
            {
                if (transformProperty.objectReferenceValue != null && transformProperty.objectReferenceValue is Transform transform)
                {
                    if (string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name))
                    {
                        transform.gameObject.SetIcon(EditorGUIUtils.DefaultButtonIconTexture);
                    }
                     else
                    {
                        transform.gameObject.SetIcon(texture);
                    }
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded)
            {
                return k_SingleLineHeightWithSpace;
            }
            return k_SingleLineHeightWithSpace * (property.FindPropertyRelative(nameof(FloatingButton.transform)).objectReferenceValue == null ? 3 : 6);
        }


        class IconWindowContent : PopupWindowContent
        {
            static class Styles
            {
                public static GUIContent noIconFound = new GUIContent("No Icon Found");

                public static readonly GUIStyle iconButton = new GUIStyle()
                {
                    imagePosition = ImagePosition.ImageAbove,
                    alignment = TextAnchor.LowerCenter,
                    stretchHeight = false,
                    fontStyle = FontStyle.Bold,
                    margin = new RectOffset(4, 4, 4, 4),
                    padding = new RectOffset(10, 10, 5, 5)
                };

            }

            public Action<Texture2D> onSelected;

            private int _selectedIndex;
            private Vector2 scrollPosition;
            public override Vector2 GetWindowSize()
            {
                Vector2 size = base.GetWindowSize();
                return EditorGUIUtils.buttonIcons.Length == 0 ? new Vector2(size.x, k_SingleLineHeightWithSpace * 2) : size;
            }

            public override void OnGUI(Rect rect)
            {
                if (Icons.BuiltInIcons.Count == 0)
                {
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, k_SingleLineHeightWithSpace), Styles.noIconFound, EditorStyles.centeredGreyMiniLabel);
                }
                using (new GUILayout.ScrollViewScope(scrollPosition, EditorStyles.helpBox))
                {
                    using (var scope = new EditorGUI.ChangeCheckScope())
                    {
                        _selectedIndex = GUI.SelectionGrid(new Rect(rect.x, rect.y, rect.width, rect.height - k_SingleLineHeightWithSpace - EditorGUIUtility.standardVerticalSpacing), _selectedIndex, EditorGUIUtils.buttonIcons, 3, Styles.iconButton);
                        if (scope.changed)
                        {
                            if (onSelected != null)
                            {
                                onSelected.Invoke((Texture2D)EditorGUIUtils.buttonIcons[_selectedIndex].image);
                                editorWindow.Close();
                            }
                        }
                    }
                }
                if (Icons.CustomIcons.Count == 0)
                {
                    EditorGUI.LabelField(new Rect(rect.x, rect.y + k_SingleLineHeightWithSpace,++ rect.width, k_SingleLineHeightWithSpace), Styles.noIconFound, EditorStyles.centeredGreyMiniLabel);
                }
                if (GUI.Button(new Rect(rect.x + 2, rect.yMax - k_SingleLineHeightWithSpace, rect.width - 4, EditorGUIUtility.singleLineHeight), "Use Default"))
                {
                    onSelected.Invoke(EditorGUIUtils.DefaultButtonIconTexture);
                    editorWindow.Close();
                }
            }
        }
    }

    [CustomEditor(typeof(IconAsset))]
    internal class IconAssetDrawer : Editor
    {
        public override bool HasPreviewGUI()
        {
            return true;
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            if ((target as IconAsset).icon.IsNullOrNone())
            {
                return;
            }
            if (Event.current.type == EventType.Repaint)
            {
                GUI.DrawTexture(r, (target as IconAsset).icon, ScaleMode.ScaleToFit);
            }
        }

        protected override bool ShouldHideOpenButton()
        {
            return true;
        }
    }
}
