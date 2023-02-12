using System.Linq;
using System.Reflection;
using Treasured.UnitySdk.Utilities;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [CustomPropertyDrawer(typeof(EnableIfAttribute), true)]
    [CustomPropertyDrawer(typeof(ShowIfAttribute), true)]
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute), true)]
    [CustomPropertyDrawer(typeof(OnValueChangedAttribute), true)]
    [CustomPropertyDrawer(typeof(ButtonAttribute), true)]
    [CustomPropertyDrawer(typeof(RequiredFieldAttribute), true)]
    [CustomPropertyDrawer(typeof(PresetAttribute), true)]
    public class AttributeDrawer : PropertyDrawer
    {
        public static class Styles
        {
            public const float MiniButtonWidth = 20;
            public const float ControlSpacing = 2;
            public static GUIContent Dropdown = EditorGUIUtility.TrIconContent("icon dropdown");
            public static GUIStyle RequiredLabel = new GUIStyle(EditorStyles.boldLabel)
            {
                normal = { textColor = Color.red },
                focused = { textColor = Color.red },
                active = { textColor = Color.red },
                hover = { textColor = Color.red },
            };
        }

        private bool _showProperty = true;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ShowIfAttribute showIfAttribute = fieldInfo.GetCustomAttribute<ShowIfAttribute>();
            if (showIfAttribute != null)
            {
                MemberInfo[] memberInfos = fieldInfo.DeclaringType.GetMember(showIfAttribute.Getter, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                if (memberInfos.Length == 1)
                {
                    var target = EditorReflectionUtilities.GetDeclaringObject(property);
                    switch (memberInfos[0])
                    {
                        case FieldInfo fieldInfo:
                            _showProperty = (bool)fieldInfo.GetValue(target);
                            break;
                        case MethodInfo methodInfo:
                            _showProperty = (bool)methodInfo.Invoke(target, null);
                            break;
                        case PropertyInfo propertyInfo:
                            _showProperty = (bool)propertyInfo.GetValue(target, null);
                            break;
                        default:
                            break;
                    }
                }
                if (!_showProperty)
                {
                    return;
                }
            }
            TextAreaAttribute textAreaAttribute = fieldInfo.GetCustomAttribute<TextAreaAttribute>();
            EnableIfAttribute enableIfAttribute = fieldInfo.GetCustomAttribute<EnableIfAttribute>();
            ReadOnlyAttribute readOnlyAttribute = fieldInfo.GetCustomAttribute<ReadOnlyAttribute>();
            var buttonAttributes = fieldInfo.GetCustomAttributes<ButtonAttribute>().ToArray();
            OnValueChangedAttribute onValueChangedAttribute = fieldInfo.GetCustomAttribute<OnValueChangedAttribute>();
            Rect total = new Rect(position.x, position.y, position.width, position.height - EditorGUIUtility.singleLineHeight * buttonAttributes.Length);
            using (new EditorGUI.DisabledGroupScope(readOnlyAttribute != null))
            {
                EditorGUI.BeginChangeCheck();
                switch (property.propertyType)
                {
                    case SerializedPropertyType.String:
                        Rect rect = PrefixLabel(new Rect(total.x, total.y, total.width, total.height), property, label);
                        if (textAreaAttribute != null)
                        {
                            property.stringValue = EditorGUI.TextArea(rect, property.stringValue);
                        }
                        else
                        {
                            property.stringValue = EditorGUI.TextField(rect, property.stringValue);
                        }
                        break;
                    default:
                        EditorGUI.PropertyField(position, property, label, true);
                        break;
                }
                if (EditorGUI.EndChangeCheck() && onValueChangedAttribute != null)
                {
                    Invoke(property, onValueChangedAttribute.CallbackName);
                }
            }
            for (int i = 0; i < buttonAttributes.Length; i++)
            {
                if (GUI.Button(new Rect(position.x, position.y + total.height + EditorGUIUtility.singleLineHeight * i, position.width, EditorGUIUtility.singleLineHeight), buttonAttributes[i].Text))
                {
                    Invoke(property, buttonAttributes[i].CallbackName);
                }
            }
        }

        void Invoke(SerializedProperty property, string name)
        {
            var target = EditorUtils.GetTargetObjectWithProperty(property);
            var callbackMethod = EditorUtils.GetMethod(target, name);
            if (callbackMethod != null && callbackMethod.ReturnType == typeof(void)
                                       && callbackMethod.GetParameters().Length == 0)
            {
                //property.serializedObject.ApplyModifiedProperties();
                //property.serializedObject.Update();
                callbackMethod.Invoke(target, new object[] { });
            }
            else
            {
                string warning = string.Format(
                    "{0} can invoke only methods with 'void' return type and 0 parameters",
                    attribute.GetType().Name);

                Debug.LogWarning(warning, property.serializedObject.targetObject);
            }
            // lose text field focus to avoid delay
            GUI.FocusControl(null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="total"></param>
        /// <param name="serializedProperty"></param>
        /// <param name="label"></param>
        /// <returns></returns>
        Rect PrefixLabel(Rect total, SerializedProperty serializedProperty, GUIContent label)
        {
            RequiredFieldAttribute requiredFieldAttribute = fieldInfo.GetCustomAttribute<RequiredFieldAttribute>();
            TextAreaAttribute textAreaAttribute = fieldInfo.GetCustomAttribute<TextAreaAttribute>();
            PresetAttribute presetAttribute = fieldInfo.GetCustomAttribute<PresetAttribute>();
            bool missingData = false;
            if (requiredFieldAttribute != null)
            {
                switch (serializedProperty.propertyType)
                {
                    case SerializedPropertyType.String:
                        missingData = string.IsNullOrWhiteSpace(serializedProperty.stringValue);
                        break;
                    case SerializedPropertyType.ObjectReference:
                        missingData = serializedProperty.objectReferenceValue.IsNullOrNone();
                        break;
                }
            }
            Rect labelRect = new Rect(total.x, total.y, total.width, EditorGUIUtility.singleLineHeight);
            bool hasTooltip = !string.IsNullOrEmpty(serializedProperty.tooltip);
            Rect controlRect = EditorGUI.PrefixLabel(labelRect, missingData ? EditorGUIUtility.TrTextContent(serializedProperty.displayName, $"This field is required.{(hasTooltip ? "\n" + serializedProperty.tooltip : "")}", "Error") : label, missingData ? Styles.RequiredLabel : EditorStyles.label);
            if (presetAttribute != null)
            {
                controlRect = new Rect(controlRect.x - 14, controlRect.y, controlRect.width + 14 - Styles.MiniButtonWidth - Styles.ControlSpacing, controlRect.height);
                if (EditorGUI.DropdownButton(new Rect(total.xMax - Styles.MiniButtonWidth, labelRect.y, Styles.MiniButtonWidth, EditorGUIUtility.singleLineHeight), Styles.Dropdown, FocusType.Passive))
                {
                    ShowPresetMenu(serializedProperty);
                }
            }
            if (textAreaAttribute == null)
            {
                // inline rect
                return controlRect;
            }
            else
            {
                return new Rect(total.x, total.y + EditorGUIUtility.singleLineHeight, total.width, total.height - EditorGUIUtility.singleLineHeight);
            }
        }

        void ShowPresetMenu(SerializedProperty property)
        {
            PresetAttribute attr = fieldInfo.GetCustomAttribute<PresetAttribute>();
            if (attr == null) return;
            GenericMenu menu = new GenericMenu();
            foreach (var item in attr.Values)
            {
                menu.AddItem(new GUIContent(item), false, () =>
                {
                    property.stringValue = item;
                    property.serializedObject.ApplyModifiedProperties();
                    GUI.FocusControl(null);
                });
            }
            menu.ShowAsContext();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!_showProperty) { return 0f; }
            float totalHeight = EditorGUI.GetPropertyHeight(property, label, true);
            // Add text area height
            if (property.propertyType == SerializedPropertyType.String)
            {
                TextAreaAttribute textAreaAttribute = fieldInfo.GetCustomAttribute<TextAreaAttribute>();
                if (textAreaAttribute != null)
                {
                    totalHeight += (textAreaAttribute.minLines) * (EditorGUIUtility.singleLineHeight);
                }
            }
            // Add button height
            totalHeight += EditorGUIUtility.singleLineHeight * fieldInfo.GetCustomAttributes<ButtonAttribute>().Count();
            return totalHeight;
        }
    }
}
