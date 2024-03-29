﻿using System;
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
    [CustomPropertyDrawer(typeof(LabelAttribute), true)]
    [CustomPropertyDrawer(typeof(DescriptionAttribute), true)]
    [CustomPropertyDrawer(typeof(TooltipAttribute), true)]
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
            public static readonly GUIStyle EnumDescription = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
            {
                wordWrap = true,
                alignment = TextAnchor.UpperLeft
            };
        }

        private bool _showProperty = true;
        private bool _disabled = false;
        private bool _isMissing = false;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            LabelAttribute labelAttribute = fieldInfo.GetCustomAttribute<LabelAttribute>();
            TooltipAttribute tooltipAttribute = fieldInfo.GetCustomAttribute<TooltipAttribute>();
            label = labelAttribute != null && !string.IsNullOrWhiteSpace(labelAttribute.Text) ? new GUIContent(labelAttribute.Text, tooltipAttribute == null ? labelAttribute.Tooltip : tooltipAttribute.tooltip) : label;
            if (tooltipAttribute != null)
            {
                label.tooltip = tooltipAttribute.tooltip;
            }
            EditorGUI.BeginProperty(position, label, property);
            ValidateInput(property);
            ShowIfAttribute showIfAttribute = fieldInfo.GetCustomAttribute<ShowIfAttribute>();
            ReadOnlyAttribute readOnlyAttribute = fieldInfo.GetCustomAttribute<ReadOnlyAttribute>();
            EnableIfAttribute enableIfAttribute = fieldInfo.GetCustomAttribute<EnableIfAttribute>();
            PresetAttribute presetAttribute = fieldInfo.GetCustomAttribute<PresetAttribute>();
            DescriptionAttribute descriptionAttribute = fieldInfo.GetCustomAttribute<DescriptionAttribute>();
            _disabled = readOnlyAttribute != null;
            if (enableIfAttribute != null)
            {
                _disabled = GetCondition(property, enableIfAttribute.Getter);
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
                GUI.SetNextControlName("AttributeDrawerControl");
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
                            if (presetAttribute != null && !presetAttribute.Customizable)
                            {
                                EditorGUI.BeginChangeCheck();
                                var index = EditorGUI.Popup(new Rect(rect.x, rect.y, rect.width, rect.height), Array.IndexOf(presetAttribute.Values, property.stringValue), presetAttribute.Values);
                                if (EditorGUI.EndChangeCheck())
                                {
                                    property.stringValue = presetAttribute.Values[index];
                                }
                            }
                            else
                            {
                                property.stringValue = EditorGUI.TextField(rect, property.stringValue);
                            }
                            EditorGUI.indentLevel = indent;
                        }
                        break;
                    default:
                        EditorGUI.PropertyField(rect, property, GUIContent.none, true);
                        break;
                }
                if (EditorGUI.EndChangeCheck())
                {
                    if (onValueChangedAttribute != null)
                    {
                        // Makes sure state is updated before invoke onValueChanged functions
                        property.serializedObject.ApplyModifiedProperties();
                        Invoke(property, onValueChangedAttribute);
                        // Text input will lose focus when ApplyModifiedProperties(), this will make it to refocus the control
                        EditorGUI.FocusTextInControl("AttributeDrawerControl");
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
            if (descriptionAttribute != null && property.propertyType == SerializedPropertyType.Enum)
            {
                var content = new GUIContent(descriptionAttribute?.Descriptions[property.enumValueIndex]);
                var rect = GUILayoutUtility.GetRect(content, Styles.EnumDescription);
                EditorGUI.LabelField(new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, rect.width, rect.height), content, Styles.EnumDescription);
            }
            EditorGUI.EndProperty();
            property.serializedObject.ApplyModifiedProperties();
            property.serializedObject.Update();
        }

        bool GetCondition(SerializedProperty property, string getter)
        {
            getter = getter.Trim();
            bool condition = false;
            bool startsWithNot = getter.StartsWith("!");
            if (startsWithNot) getter = getter.Substring(1);
            MemberInfo[] memberInfos = fieldInfo.DeclaringType.GetMember(getter, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            if (memberInfos.Length == 1)
            {
                var target = EditorReflectionUtilities.GetDeclaringObject(property);
                object obj = null;
                switch (memberInfos[0])
                {
                    case FieldInfo fieldInfo:
                        obj = fieldInfo.GetValue(target);
                        break;
                    case MethodInfo methodInfo:
                        obj = methodInfo.Invoke(target, null);
                        break;
                    case PropertyInfo propertyInfo:
                        obj = propertyInfo.GetValue(target, null);
                        break;
                    default:
                        break;
                }
                if (obj is UnityEngine.Object unityObj)
                {
                    condition = !unityObj.IsNullOrNone();
                }
                else if (obj != null && obj.GetType() == typeof(bool))
                {
                    condition = startsWithNot ? !(bool)obj : (bool)obj;
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
            Rect controlRect = EditorGUI.PrefixLabel(labelRect, new GUIContent(!_isMissing ? label.text : $"{label.text} <color={Styles.RequiredColorHex}>*</color>", property.tooltip), Styles.RichText);
            if (textAreaAttribute != null)
            {
                controlRect = new Rect(total.x, total.y + EditorGUIUtility.singleLineHeight, total.width, total.height - EditorGUIUtility.singleLineHeight - (_isMissing ? EditorGUIUtility.singleLineHeight : 0));
            }
            if (presetAttribute != null && presetAttribute.Customizable)
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
            OnValueChangedAttribute onValueChangedAttribute = fieldInfo.GetCustomAttribute<OnValueChangedAttribute>();
            if (attr == null) return;
            GenericMenu menu = new GenericMenu();
            foreach (var item in attr.Values)
            {
                menu.AddItem(new GUIContent(item), false, () =>
                {
                    property.stringValue = item;
                    property.serializedObject.ApplyModifiedProperties();
                    if (onValueChangedAttribute != null)
                    {
                        Invoke(property, onValueChangedAttribute);
                    }
                    // unfocus the control so gui gets updated
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
            DescriptionAttribute descriptionAttribute = fieldInfo.GetCustomAttribute<DescriptionAttribute>();
            // Add text area height
            switch (property.propertyType)
            {
                case SerializedPropertyType.String:
                    if (textAreaAttribute != null)
                    {
                        totalHeight += (textAreaAttribute.minLines) * (EditorGUIUtility.singleLineHeight);
                    }
                    break;
                case SerializedPropertyType.Enum:
                    var content = new GUIContent(descriptionAttribute?.Descriptions[property.enumValueIndex]);
                    var height = Styles.EnumDescription.CalcSize(content).y;
                    totalHeight += height;
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
