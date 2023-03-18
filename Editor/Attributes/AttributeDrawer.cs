using System;
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
            public static readonly string RequiredColorHex = "#FF6E40";
            public static readonly Color RequiredColor = new Color(1, 110 / 255f, 64 / 255f);
            public const float MiniButtonWidth = 20;
            public const float ControlSpacing = 2;
            public static readonly GUIContent Dropdown = EditorGUIUtility.TrIconContent("icon dropdown");
            public static readonly GUIStyle RichText = new GUIStyle(EditorStyles.label)
            {
                richText = true
            };
        }

        private bool _showProperty = true;
        private bool _disabled = false;
        private bool _isMissing = false;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            property.serializedObject.Update();
            ValidateInput(property);
            ShowIfAttribute showIfAttribute = fieldInfo.GetCustomAttribute<ShowIfAttribute>();
            ReadOnlyAttribute readOnlyAttribute = fieldInfo.GetCustomAttribute<ReadOnlyAttribute>();
            EnableIfAttribute enableIfAttribute = fieldInfo.GetCustomAttribute<EnableIfAttribute>();
            _disabled = readOnlyAttribute != null;
            if (enableIfAttribute != null)
            {
                _disabled = _disabled == GetCondition(property, enableIfAttribute.Getter);
            }
            RequiredFieldAttribute requiredFieldAttribute = fieldInfo.GetCustomAttribute<RequiredFieldAttribute>();
            if (showIfAttribute != null)
            {
                _showProperty = GetCondition(property, showIfAttribute.Getter);
                if (!_showProperty)
                {
                    return;
                }
            }
            TextAreaAttribute textAreaAttribute = fieldInfo.GetCustomAttribute<TextAreaAttribute>();
            var buttonAttributes = fieldInfo.GetCustomAttributes<ButtonAttribute>().ToArray();
            OnValueChangedAttribute onValueChangedAttribute = fieldInfo.GetCustomAttribute<OnValueChangedAttribute>();
            Rect total = new Rect(position.x, position.y, position.width, position.height - EditorGUIUtility.singleLineHeight * buttonAttributes.Length);
            using (new EditorGUI.DisabledGroupScope(_disabled))
            {
                EditorGUI.BeginChangeCheck();
                Rect rect = PrefixLabel(new Rect(total.x, total.y, total.width, total.height), property, label);
                switch (property.propertyType)
                {
                    case SerializedPropertyType.String:
                        
                        if (textAreaAttribute != null)
                        {
                            property.stringValue = EditorGUI.TextArea(rect, property.stringValue);
                        }
                        else
                        {
                            int indent = EditorGUI.indentLevel;
                            EditorGUI.indentLevel = 0;
                            property.stringValue = EditorGUI.TextField(rect, property.stringValue);
                            EditorGUI.indentLevel = indent;
                        }
                        break;
                    default:
                        property.serializedObject.Update();
                        EditorGUI.PropertyField(rect, property, GUIContent.none, true);
                        property.serializedObject.ApplyModifiedProperties();
                        break;
                }
                if (EditorGUI.EndChangeCheck())
                {
                    if (onValueChangedAttribute != null)
                    {
                        Invoke(property, onValueChangedAttribute);
                    }
                    ValidateInput(property);
                }
                if (_isMissing)
                {
                    EditorGUI.LabelField(new Rect(total.x, total.yMax - EditorGUIUtility.singleLineHeight, total.width, EditorGUIUtility.singleLineHeight), $"<color={Styles.RequiredColorHex}>{requiredFieldAttribute.Text}</color>", Styles.RichText);
                }
            }
            for (int i = 0; i < buttonAttributes.Length; i++)
            {
                if (GUI.Button(new Rect(position.x, position.y + total.height + EditorGUIUtility.singleLineHeight * i, position.width, EditorGUIUtility.singleLineHeight), buttonAttributes[i].Text))
                {
                    Invoke(property, buttonAttributes[i]);
                }
            }
            property.serializedObject.ApplyModifiedProperties();
            EditorGUI.EndProperty();
        }

        bool GetCondition(SerializedProperty property, string getter)
        {
            bool condition = false;
            MemberInfo[] memberInfos = fieldInfo.DeclaringType.GetMember(getter, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            if (memberInfos.Length == 1)
            {
                var target = EditorReflectionUtilities.GetDeclaringObject(property);
                switch (memberInfos[0])
                {
                    case FieldInfo fieldInfo:
                        condition = (bool)fieldInfo.GetValue(target);
                        break;
                    case MethodInfo methodInfo:
                        condition = (bool)methodInfo.Invoke(target, null);
                        break;
                    case PropertyInfo propertyInfo:
                        condition = (bool)propertyInfo.GetValue(target, null);
                        break;
                    default:
                        break;
                }
            }
            return condition;
        }

        void Invoke(SerializedProperty property, Attribute attribute)
        {
            var target = EditorUtils.GetTargetObjectWithProperty(property);
            IMethodInvoker invoker = attribute as IMethodInvoker;
            MethodInfo callbackMethod = invoker == null ? null : EditorUtils.GetMethod(target, invoker.CallbackName);
            if (callbackMethod != null)
            {
                if (callbackMethod.GetParameters().Length == 0)
                {
                    callbackMethod.Invoke(target, new object[] { });
                }
                else
                {
                    string warning = string.Format(
                    "{0} can invoke only methods with 0 parameters",
                    attribute.GetType().Name);

                    Debug.LogWarning(warning, property.serializedObject.targetObject);
                }
            }
            else
            {
                Debug.LogWarning($"Method not found. Callback name: {invoker.CallbackName}", property.serializedObject.targetObject);
            }
            // lose text field focus to avoid delay
            GUI.FocusControl(null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="total"></param>
        /// <param name="property"></param>
        /// <param name="label"></param>
        /// <returns></returns>
        Rect PrefixLabel(Rect total, SerializedProperty property, GUIContent label)
        {
            TextAreaAttribute textAreaAttribute = fieldInfo.GetCustomAttribute<TextAreaAttribute>();
            PresetAttribute presetAttribute = fieldInfo.GetCustomAttribute<PresetAttribute>();
            Rect labelRect = new Rect(total.x, total.y, total.width, EditorGUIUtility.singleLineHeight);
            // inline rect
            Rect controlRect = EditorGUI.PrefixLabel(labelRect, new GUIContent(!_isMissing ? property.displayName : $"{property.displayName} <color={Styles.RequiredColorHex}>*</color>", property.tooltip), Styles.RichText);
            if (textAreaAttribute != null)
            {
                controlRect = new Rect(total.x, total.y + EditorGUIUtility.singleLineHeight, total.width, total.height - EditorGUIUtility.singleLineHeight - (_isMissing ? EditorGUIUtility.singleLineHeight : 0));
            }
            if (presetAttribute != null)
            {
                controlRect = new Rect(controlRect.x, controlRect.y, controlRect.width - Styles.MiniButtonWidth - Styles.ControlSpacing, controlRect.height);
                if (EditorGUI.DropdownButton(new Rect(total.xMax - Styles.MiniButtonWidth, labelRect.y, Styles.MiniButtonWidth, EditorGUIUtility.singleLineHeight), Styles.Dropdown, FocusType.Passive))
                {
                    ShowPresetMenu(property);
                }
            }
            return controlRect;
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
            if (!_showProperty) { return -EditorGUIUtility.standardVerticalSpacing; }
            float totalHeight = EditorGUI.GetPropertyHeight(property, label, true);
            TextAreaAttribute textAreaAttribute = fieldInfo.GetCustomAttribute<TextAreaAttribute>();
            // Add text area height
            switch (property.propertyType)
            {
                case SerializedPropertyType.String:
                    if (textAreaAttribute != null)
                    {
                        totalHeight += (textAreaAttribute.minLines) * (EditorGUIUtility.singleLineHeight);
                    }
                    break;
                default:
                    break;
            }
            if (_isMissing) totalHeight += EditorGUIUtility.singleLineHeight;
            // Add button height
            totalHeight += EditorGUIUtility.singleLineHeight * fieldInfo.GetCustomAttributes<ButtonAttribute>().Count();
            return totalHeight;
        }

        private void ValidateInput(SerializedProperty serializedProperty)
        {
            RequiredFieldAttribute requiredFieldAttribute = fieldInfo.GetCustomAttribute<RequiredFieldAttribute>();
            if (requiredFieldAttribute != null)
            {
                switch (serializedProperty.propertyType)
                {
                    case SerializedPropertyType.String:
                        _isMissing = string.IsNullOrWhiteSpace(serializedProperty.stringValue);
                        break;
                    case SerializedPropertyType.ObjectReference:
                        _isMissing = serializedProperty.objectReferenceValue.IsNullOrNone();
                        break;
                }
            }
        }
    }
}
