using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk.Editor
{
    [CustomPropertyDrawer(typeof(CodeAttribute))]
    public class CodeAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect buttonRect = EditorGUI.PrefixLabel(position, label);
            if (GUI.Button(buttonRect, new GUIContent("Edit")))
            {
                CodeEditorWindow.Show(property, (CodeAttribute)attribute);
            }
        }

        private class CodeEditorWindow : EditorWindow
        {
            private SerializedProperty serializedProperty;
            private CodeAttribute attribute;
            private Vector2 previewScrollPosition;
            private Vector2 textScrollPosition;

            static string s_brown = "#964B00";
            static string s_blue = "#3399ff";

            Dictionary<string, string> _tagColorPairs = new Dictionary<string, string>
            {
                { "iframe", s_blue },
                { "width", s_brown },
                { "height", s_brown},
                { "src", s_brown },
                { "script", s_blue },
                { "title", s_brown },
                { "frameborder", s_brown },
                { "allow", s_brown },
                { "allowfullscreen", s_brown },
            };

            public static void Show(SerializedProperty serializedProperty, CodeAttribute attribute)
            {
                CodeEditorWindow window = EditorWindow.GetWindow<CodeEditorWindow>(true, "Code Editor");
                window.minSize = new Vector2(500, 300);
                window.serializedProperty = serializedProperty;
                window.attribute = attribute;
                window.Show();
            }

            private string PraseHTML(string text)
            {
                string result = text;
                foreach (var item in _tagColorPairs)
                {
                    var regex = new Regex("([^A-z0-9])(" + item.Key + ")([^A-z0-9])");
                    result = regex.Replace(result, $"$1<color={item.Value}>$2</color>$3");
                }
                return result;
            }

            private void OnGUI()
            {
                if (serializedProperty == null || serializedProperty.propertyType != SerializedPropertyType.String)
                {
                    return;
                }
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUI.BeginChangeCheck();
                    using (new EditorGUILayout.VerticalScope(GUILayout.Width(this.position.width / 2)))
                    {
                        EditorGUILayout.LabelField("Code");
                        EditorStyles.textArea.richText = false;
                        Rect rect = EditorGUILayout.GetControlRect(GUILayout.ExpandHeight(true));
                        var args = new object[] { rect, serializedProperty.stringValue, textScrollPosition, EditorStyles.textArea };
                        serializedProperty.stringValue = (string)typeof(EditorGUI).GetMethod("ScrollableTextAreaInternal", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, args);
                        textScrollPosition = (Vector2)args[2]; // set the value of scroll position
                        var buttons = typeof(CodeEditorWindow).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(x => x.GetParameters().Length == 0 && x.IsDefined(typeof(ButtonAttribute)));
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            foreach (MethodInfo button in buttons)
                            {
                                if (GUILayout.Button(ObjectNames.NicifyVariableName(button.Name)))
                                {
                                    button.Invoke(this, null);
                                }
                            }
                        }
                    }
                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedProperty.serializedObject.ApplyModifiedProperties();
                    }
                    using (new EditorGUILayout.VerticalScope())
                    {
                        EditorGUILayout.LabelField("Syntax Highlighting(Experimental)");
                        EditorStyles.textArea.wordWrap = true;
                        if (attribute != null)
                        {
                            EditorStyles.textArea.richText = true;
                            Rect previewRect = EditorGUILayout.GetControlRect(GUILayout.ExpandHeight(true));
                            string html = PraseHTML(serializedProperty.stringValue);
                            var previewArgs = new object[] { previewRect, html, previewScrollPosition, EditorStyles.textArea };
                            typeof(EditorGUI).GetMethod("ScrollableTextAreaInternal", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, previewArgs);
                            previewScrollPosition = (Vector2)previewArgs[2]; // set the value of scroll position
                        }
                    }
                }
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
                serializedProperty.serializedObject.ApplyModifiedProperties();
            }
        }
    }

  
}
