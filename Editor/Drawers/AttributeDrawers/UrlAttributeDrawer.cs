using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [CustomPropertyDrawer(typeof(UrlAttribute))]
    public class UrlAttributeDrawer : PropertyDrawer
    {
        public class Styles
        {
            public static GUIContent menu = EditorGUIUtility.TrIconContent("_menu");
        }

        private static readonly Regex RegexSrc = new Regex("src=[\"'](.+?)[\"']");

        private static float kSingleLineHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
       
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.LabelField(new Rect(position.x, position.y, position.width - 20, kSingleLineHeight), label);
            if (GUI.Button(new Rect(position.xMax - 20, position.y, 20, EditorGUIUtility.singleLineHeight), Styles.menu, EditorStyles.label))
            {
                ShowMenu(property);
            }
            property.stringValue = EditorGUI.TextArea(new Rect(position.x, position.y + kSingleLineHeight, position.width, kSingleLineHeight * 2), property.stringValue, EditorStyles.textArea);
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return (kSingleLineHeight) * 3;
        }

        private void ShowMenu(SerializedProperty serializedProperty)
        {
            GenericMenu menu = new GenericMenu();
            var buttons = this.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(x => x.GetParameters().Length == 1 && x.IsDefined(typeof(ButtonAttribute)));
            foreach (MethodInfo button in buttons)
            {
                ButtonAttribute attr = button.GetCustomAttribute<ButtonAttribute>();
                menu.AddItem(new GUIContent(ObjectNames.NicifyVariableName(string.IsNullOrWhiteSpace(attr.Text) ? button.Name : attr.Text)), false, () =>
                {
                    button.Invoke(this, new object[] { serializedProperty });
                });
            }
            menu.ShowAsContext();
        }

        [Button("OpenInBrowser")]
        void OpenInBrowser(SerializedProperty serializedProperty)
        {
            Application.OpenURL(serializedProperty.stringValue);
        }

        [Button("Copy")]
        void Copy(SerializedProperty serializedProperty)
        {
            GUIUtility.systemCopyBuffer = serializedProperty.stringValue;
        }

        [Button("Paste")]
        void Paste(SerializedProperty serializedProperty)
        {
            serializedProperty.stringValue = GUIUtility.systemCopyBuffer;
            GUI.FocusControl(null);
        }

        [Button("Extract Src and Paste")]
        void ExtractSrcAndPaste(SerializedProperty serializedProperty)
        {
            string embed = GUIUtility.systemCopyBuffer;
            Match match = RegexSrc.Match(embed);
            if (!match.Success)
            {
                EditorUtility.DisplayDialog("No match found", "No match found. Make sure src is enclosed in quotes.", "OK");
                return;
            }
            // Groups[0] will be a string that matches the entire regular expression pattern
            // Groups[1] will be (.+?)
            string src = match.Groups[1].Value;
            if (!src.Contains(" "))
            {
                serializedProperty.stringValue = src;
                GUI.FocusControl(null);
            }
        }
    }
}
