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
        private static float k_SingleLineHeightWithSpace = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty icon = property.FindPropertyRelative(nameof(Button.icon));
            SerializedProperty transform = property.FindPropertyRelative(nameof(Button.transform));
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.LabelField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), label, EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope(1))
            {
                EditorGUI.PropertyField(new Rect(position.x, position.y + k_SingleLineHeightWithSpace, position.width - 22, EditorGUIUtility.singleLineHeight), icon);
                Rect buttonRect = new Rect(position.xMax - 20, position.y + k_SingleLineHeightWithSpace, 20, EditorGUIUtility.singleLineHeight);
                if (EditorGUI.DropdownButton(buttonRect, EditorGUIUtility.TrIconContent("icon dropdown"), FocusType.Passive))
                {
                    PopupWindow.Show(buttonRect, new IconWindowContent()
                    {
                        onSelected = (selection) =>
                        {
                            icon.stringValue = selection;
                            icon.serializedObject.ApplyModifiedProperties();
                        }
                    });
                }
                if (transform.objectReferenceValue == null)
                {
                    EditorGUI.PropertyField(new Rect(position.x, position.y + k_SingleLineHeightWithSpace * 2, position.width - 60, EditorGUIUtility.singleLineHeight), transform);
                    if (GUI.Button(new Rect(position.xMax - 58, position.y + k_SingleLineHeightWithSpace * 2, 58, EditorGUIUtility.singleLineHeight), new GUIContent("Create")))
                    {
                        Component component = transform.serializedObject.targetObject as Component;
                        if (component)
                        {
                            GameObject go = new GameObject(ObjectNames.NicifyVariableName(property.name));
                            go.transform.parent = component.transform;
                            transform.objectReferenceValue = go.transform;
                        }
                    }
                }
                else
                {
                    EditorGUI.PropertyField(new Rect(position.x, position.y + k_SingleLineHeightWithSpace * 2, position.width, EditorGUIUtility.singleLineHeight), transform);
                }
            }
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 3 + EditorGUIUtility.standardVerticalSpacing;
        }


        class IconWindowContent : PopupWindowContent
        {
            static class Styles
            {
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

            private static GUIContent[] s_iconTextures;

            public Action<string> onSelected;
            private int _selectedIndex;
            private Vector2 scrollPosition;

            public override void OnGUI(Rect rect)
            {
                using(new GUILayout.ScrollViewScope(scrollPosition, EditorStyles.helpBox))
                {
                    using (var scope = new EditorGUI.ChangeCheckScope())
                    {
                        _selectedIndex = GUI.SelectionGrid(rect, _selectedIndex, s_iconTextures, 3, Styles.iconButton);
                        if (scope.changed)
                        {
                            if (onSelected != null)
                            {
                                onSelected.Invoke(s_iconTextures[_selectedIndex].tooltip);
                                editorWindow.Close();
                            }
                        }
                    }
                }
            }

            [InitializeOnLoadMethod]
            static void LoadIcons()
            {
                // TODO : Group Icons from other source other than fontawesome
                string[] relativeIconPaths = Directory.GetFiles(Path.GetFullPath("Packages/com.treasured.unitysdk/Resources/Icons/Button/Font Awesome"), "Fa*.png").Select(x => x.Substring(x.IndexOf("Packages")).Replace("\\", "/")).ToArray();
                s_iconTextures = relativeIconPaths.Select(path =>
                            EditorGUIUtility.TrTextContentWithIcon(String.Empty, Path.GetFileNameWithoutExtension(path), AssetDatabase.LoadAssetAtPath<Texture2D>(path))).ToArray();
            }
        }
    }
}
