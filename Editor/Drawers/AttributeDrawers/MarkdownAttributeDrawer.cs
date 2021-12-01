using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk.Editor
{
    [CustomPropertyDrawer(typeof(MarkdownAttribute))]
    public class MarkdownAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.xMax -= 40;
            EditorGUI.LabelField(position, label, new GUIContent(property.stringValue, property.stringValue), EditorStyles.linkLabel);
            Rect buttonRect = new Rect(position.xMax, position.y, 40, position.height);
            if (GUI.Button(buttonRect, new GUIContent("Edit")))
            {
                MarkdownEditorWindow.Show(property, (MarkdownAttribute)attribute);
            }
        }

        private class MarkdownEditorWindow : EditorWindow
        {
            private SerializedProperty serializedProperty;
            private MarkdownAttribute attribute;
            private Vector2 previewScrollPosition;
            private Vector2 textScrollPosition;

            public static void Show(SerializedProperty serializedProperty, MarkdownAttribute attribute)
            {
                MarkdownEditorWindow window = EditorWindow.GetWindow<MarkdownEditorWindow>(true, "HTML Editor");
                window.minSize = new Vector2(500, 300);
                window.serializedProperty = serializedProperty;
                window.attribute = attribute;
                window.Show();
            }

            private void OnGUI()
            {
                if (serializedProperty == null || serializedProperty.propertyType != SerializedPropertyType.String)
                {
                    return;
                }
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.LabelField("Preview");
                EditorStyles.textArea.wordWrap = true;
                if(attribute != null)
                {
                    EditorStyles.textArea.richText = true;
                    Rect previewRect = EditorGUILayout.GetControlRect(GUILayout.ExpandHeight(true));
                    var previewArgs = new object[] { previewRect, serializedProperty.stringValue, previewScrollPosition, EditorStyles.textArea };
                    typeof(EditorGUI).GetMethod("ScrollableTextAreaInternal", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, previewArgs);
                    previewScrollPosition = (Vector2)previewArgs[2]; // set the value of scroll position
                }
                EditorGUILayout.LabelField("Content");
                EditorStyles.textArea.richText = false;
                Rect rect = EditorGUILayout.GetControlRect(GUILayout.ExpandHeight(true));
                var args = new object[] { rect, serializedProperty.stringValue, textScrollPosition, EditorStyles.textArea };
                serializedProperty.stringValue = (string)typeof(EditorGUI).GetMethod("ScrollableTextAreaInternal", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, args);
                textScrollPosition = (Vector2)args[2]; // set the value of scroll position

                var buttons = typeof(MarkdownAttribute).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(x => x.GetParameters().Length == 0 && x.IsDefined(typeof(ButtonAttribute)));
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    foreach (MethodInfo button in buttons)
                    {
                        if (GUILayout.Button(ObjectNames.NicifyVariableName(button.Name)))
                        {
                            button.Invoke(this, null);
                        }
                    }
                }    
                if (EditorGUI.EndChangeCheck())
                {
                    serializedProperty.serializedObject.ApplyModifiedProperties();
                }
            }

            private void OnLostFocus()
            {
                Close();
            }

            [Button]
            void Copy()
            {
                GUIUtility.systemCopyBuffer = serializedProperty.stringValue;
            }

            [Button]
            void Paste()
            {
                serializedProperty.stringValue = GUIUtility.systemCopyBuffer;
            }
        }
    }

  
}
