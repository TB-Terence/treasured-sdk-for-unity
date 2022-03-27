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
            SerializedProperty iconProperty = property.FindPropertyRelative(nameof(Button.icon));
            SerializedProperty transformProperty = property.FindPropertyRelative(nameof(Button.transform));
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.LabelField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), label, EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope(1))
            {
                EditorGUI.PropertyField(new Rect(position.x, position.y + k_SingleLineHeightWithSpace, position.width - 22, EditorGUIUtility.singleLineHeight), iconProperty);
                Rect buttonRect = new Rect(position.xMax - 20, position.y + k_SingleLineHeightWithSpace, 20, EditorGUIUtility.singleLineHeight);
                if (EditorGUI.DropdownButton(buttonRect, EditorGUIUtility.TrIconContent("icon dropdown"), FocusType.Passive))
                {
                    PopupWindow.Show(buttonRect, new IconWindowContent()
                    {
                        onSelected = (selection) =>
                        {
                            iconProperty.stringValue = selection == null ? String.Empty : selection.name;
                            if (transformProperty.objectReferenceValue != null && transformProperty.objectReferenceValue is Transform transform)
                            {
                                transform.gameObject.SetIcon(selection);
                            }
                            iconProperty.serializedObject.ApplyModifiedProperties();
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
                            transform.localScale = EditorGUI.Vector3Field(new Rect(position.x, position.y + k_SingleLineHeightWithSpace * 4, position.width, EditorGUIUtility.singleLineHeight), "Size", transform.localScale);
                            if (scope.changed)
                            {
                                transformProperty.serializedObject.ApplyModifiedProperties();
                            }
                        }
                    }
                }
            }
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 5 + EditorGUIUtility.standardVerticalSpacing;
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

            public Action<Texture2D> onSelected;

            private Texture2D[] _icons;
            private int _selectedIndex;
            private Vector2 scrollPosition;

            public IconWindowContent()
            {
                string[] relativeIconPaths = Directory.GetFiles(Path.GetFullPath("Packages/com.treasured.unitysdk/Resources/Icons/Objects/Font Awesome"), "Fa*.png").Select(x => x.Substring(x.IndexOf("Packages")).Replace("\\", "/")).ToArray();
                _icons = relativeIconPaths.Select(path => AssetDatabase.LoadAssetAtPath<Texture2D>(path)).ToArray();
            }

            public override void OnGUI(Rect rect)
            {
                using(new GUILayout.ScrollViewScope(scrollPosition, EditorStyles.helpBox))
                {
                    using (var scope = new EditorGUI.ChangeCheckScope())
                    {
                        _selectedIndex = GUI.SelectionGrid(new Rect(rect.x, rect.y, rect.width,  rect.height - k_SingleLineHeightWithSpace - EditorGUIUtility.standardVerticalSpacing), _selectedIndex, _icons, 3, Styles.iconButton);
                        if (scope.changed)
                        {
                            if (onSelected != null)
                            {
                                onSelected.Invoke((Texture2D)_icons[_selectedIndex]);
                                editorWindow.Close();
                            }
                        }
                    }
                }
                if (GUI.Button(new Rect(rect.x + 2, rect.yMax - k_SingleLineHeightWithSpace, rect.width - 4, EditorGUIUtility.singleLineHeight), "Use Default"))
                {
                    onSelected.Invoke(null);
                    editorWindow.Close();
                }
            }
        }
    }
}
