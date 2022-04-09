using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [CustomPropertyDrawer(typeof(Button))]
    internal class ButtonDrawer : PropertyDrawer
    {
        /// <summary>
        /// Filter for Icons.
        /// Finds all asset's file name contains Fa and is type texture2D.
        /// </summary>
        private const string kIconFilter = "Fa t:texture2D";
        private static string[] s_IconFolderPaths = new string[] { "Packages/com.treasured.unitysdk/Resources/Icons/Objects/Font Awesome" };
        private static float k_SingleLineHeightWithSpace = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        private static GUIContent[] s_icons;
        private static GUIContent s_defaultIcon;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty iconProperty = property.FindPropertyRelative(nameof(Button.icon));
            SerializedProperty transformProperty = property.FindPropertyRelative(nameof(Button.transform));
            EditorGUI.BeginProperty(position, label, property);
            property.isExpanded = EditorGUI.BeginFoldoutHeaderGroup(new Rect(position.x, position.y, position.width, k_SingleLineHeightWithSpace), property.isExpanded, label);
            if (property.isExpanded)
            {
                using (new EditorGUI.IndentLevelScope(1))
                {
                    using (var changeCheckScope = new EditorGUI.ChangeCheckScope())
                    {
                        EditorGUI.PropertyField(new Rect(position.x, position.y + k_SingleLineHeightWithSpace, position.width - 22, EditorGUIUtility.singleLineHeight), iconProperty);
                        if (changeCheckScope.changed && (string.IsNullOrWhiteSpace(iconProperty.stringValue) || string.IsNullOrEmpty(iconProperty.stringValue)))
                        {
                            SetIcon(property, "FaCircle");
                        }
                    }
                    Rect buttonRect = new Rect(position.xMax - 20, position.y + k_SingleLineHeightWithSpace, 20, EditorGUIUtility.singleLineHeight);
                    if (EditorGUI.DropdownButton(buttonRect, EditorGUIUtility.TrIconContent("icon dropdown"), FocusType.Passive))
                    {
                        PopupWindow.Show(buttonRect, new IconWindowContent()
                        {
                            onSelected = (selection) =>
                            {
                                SetIcon(property, selection == null ? "FaCircle" : selection.name);
                            }
                        });
                    }
                    if (transformProperty.objectReferenceValue == null)
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
                                go.transform.localScale = Vector3.zero;
                                transformProperty.objectReferenceValue = go.transform;
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
            SerializedProperty iconProperty = property.FindPropertyRelative(nameof(Button.icon));
            SerializedProperty transformProperty = property.FindPropertyRelative(nameof(Button.transform));
            Texture2D texture = (Texture2D)s_icons.FirstOrDefault(x => x.tooltip == name).image;
            iconProperty.stringValue = name;
            if (texture != null)
            {
                if (transformProperty.objectReferenceValue != null && transformProperty.objectReferenceValue is Transform transform)
                {
                    if (string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name))
                    {
                        transform.gameObject.SetIcon((Texture2D)s_defaultIcon.image);
                    }
                    else
                    {
                        transform.gameObject.SetIcon(texture);
                    }
                }
            }
            iconProperty.serializedObject.ApplyModifiedProperties();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded)
            {
                return k_SingleLineHeightWithSpace;
            }
            return k_SingleLineHeightWithSpace * (property.FindPropertyRelative(nameof(Button.transform)).objectReferenceValue == null ? 3 : 6);
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

            public IconWindowContent()
            {
                var guids = AssetDatabase.FindAssets(kIconFilter, s_IconFolderPaths);
                if (s_icons == null || guids.Length != s_icons.Length)
                {
                    s_icons = new GUIContent[guids.Length];
                    for (int i = 0; i < guids.Length; i++)
                    {
                        string guid = guids[i];
                        string texturePath = AssetDatabase.GUIDToAssetPath(guid);
                        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                        s_icons[i] = new GUIContent(string.Empty, texture, texture.name);
                    }
                    s_defaultIcon = s_icons.FirstOrDefault(guiContent => guiContent.tooltip == "FaCircle");
                }
            }

            public override Vector2 GetWindowSize()
            {
                Vector2 size = base.GetWindowSize();
                return s_icons.Length == 0 ? new Vector2(size.x, k_SingleLineHeightWithSpace * 2) : size;
            }

            public override void OnGUI(Rect rect)
            {
                if (s_icons.Length == 0)
                {
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, k_SingleLineHeightWithSpace), Styles.noIconFound, EditorStyles.centeredGreyMiniLabel);
                }
                else
                {
                    using (new GUILayout.ScrollViewScope(scrollPosition, EditorStyles.helpBox))
                    {
                        using (var scope = new EditorGUI.ChangeCheckScope())
                        {
                            _selectedIndex = GUI.SelectionGrid(new Rect(rect.x, rect.y, rect.width, rect.height - k_SingleLineHeightWithSpace - EditorGUIUtility.standardVerticalSpacing), _selectedIndex, s_icons, 3, Styles.iconButton);
                            if (scope.changed)
                            {
                                if (onSelected != null)
                                {
                                    onSelected.Invoke((Texture2D)s_icons[_selectedIndex].image);
                                    editorWindow.Close();
                                }
                            }
                        }
                    }
                }
                if (GUI.Button(new Rect(rect.x + 2, rect.yMax - k_SingleLineHeightWithSpace, rect.width - 4, EditorGUIUtility.singleLineHeight), "Use Default"))
                {
                    onSelected.Invoke((Texture2D)s_defaultIcon.image);
                    editorWindow.Close();
                }
            }
        }
    }
}
