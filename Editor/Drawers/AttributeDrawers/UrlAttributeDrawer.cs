using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [CustomPropertyDrawer(typeof(UrlAttribute))]
    public class UrlAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.xMax -= 40;
            EditorGUI.LabelField(position, label, new GUIContent(property.stringValue, property.stringValue), EditorStyles.linkLabel);
            Rect buttonRect = new Rect(position.xMax, position.y, 40, position.height);
            if (GUI.Button(buttonRect, new GUIContent("Edit")))
            {
                UrlEditorWindow.Show(property, (UrlAttribute)attribute);
            }
        }

        private class UrlEditorWindow : EditorWindow
        {
            private SerializedProperty serializedProperty;
            private UrlAttribute attribute;
            private Vector2 previewScrollPosition;
            private Vector2 textScrollPosition;

            public static void Show(SerializedProperty serializedProperty, UrlAttribute attribute)
            {
                UrlEditorWindow window = EditorWindow.GetWindow<UrlEditorWindow>(true, "Url Editor");
                window.minSize = new Vector2(500, 200);
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
                EditorStyles.textArea.wordWrap = true;
                EditorStyles.textArea.richText = false;
                Rect rect = EditorGUILayout.GetControlRect(GUILayout.ExpandHeight(true));
                var args = new object[] { rect, serializedProperty.stringValue, textScrollPosition, EditorStyles.textArea };
                serializedProperty.stringValue = (string)typeof(EditorGUI).GetMethod("ScrollableTextAreaInternal", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, args);
                textScrollPosition = (Vector2)args[2]; // set the value of scroll position

                var buttons = typeof(UrlEditorWindow).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(x => x.GetParameters().Length == 0 && x.IsDefined(typeof(ButtonAttribute)));
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
            void OpenInBrowser()
            {
                Application.OpenURL(serializedProperty.stringValue);
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
                GUI.FocusControl(null);
            }

            [Button]
            void ExtractSrcAndPaste()
            {
                string embed = GUIUtility.systemCopyBuffer;
                int startIndex = embed.IndexOf("src=\"") + 5;
                if (startIndex == -1 || startIndex == embed.Length)
                {
                    return;
                }
                int endIndex = embed.IndexOf('\"', startIndex);
                if(endIndex == -1)
                {
                    return;
                }
                string src = embed.Substring(startIndex, endIndex - startIndex);
                if (!src.Contains(" "))
                {
                    serializedProperty.stringValue = src;
                    GUI.FocusControl(null);
                }
            }
        }
    }

  
}
