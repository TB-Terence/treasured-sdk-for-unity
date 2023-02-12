using System;
using UnityEngine;

namespace Treasured.UnitySdk
{
    /// <summary>
    /// EnableIf is used for enable or disable the property in the inspector if field is not null.
    /// </summary>
    public class EnableIfAttribute : PropertyAttribute
    {
        public string ConditionalField = "";

        public EnableIfAttribute(string conditionalField)
        {
            ConditionalField = conditionalField;
        }
    }

    public class ShowIfAttribute : PropertyAttribute
    {
        public string ConditionalField = "";
        public string ValueToCompare = "";

        /// <summary>
        /// Show the field if the string field matches with the provided condition
        /// </summary>
        /// <param name="conditionalField">The name of the string field that will be in control</param>
        /// <param name="valueToCompare">Compare with the string value</param>
        public ShowIfAttribute(string conditionalField, string valueToCompare) : base()
        {
            this.ConditionalField = conditionalField.ToLower();
            this.ValueToCompare = valueToCompare.ToLower();
        }
    }

    public class ReadOnlyAttribute : PropertyAttribute { }
    public class RequiredFieldAttribute : PropertyAttribute { }

    public class PresetAttribute : PropertyAttribute
    {
        public string[] Values { get; private set; }
        public PresetAttribute(params string[] values)
        {
            Values = values;
        }
    }

    /// <summary>
    /// OnValueChanged attribute calls the specified function whenever the reference value has been changed via the inspector.
    /// </summary>
    public class OnValueChangedAttribute : PropertyAttribute
    {
        public string CallbackName { get; private set; }

        public OnValueChangedAttribute(string callbackName)
        {
            CallbackName = callbackName;
        }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field, AllowMultiple = true)]
    public class ButtonAttribute : PropertyAttribute
    {
        public string Text { get; private set; }
        public string CallbackName { get; private set; }
        internal bool IsCallbackFunction { get; private set; } = false;

        public ButtonAttribute(string text) : this()
        {
            Text = text;
            IsCallbackFunction = true;
        }

        public ButtonAttribute(string text, string callbackName) : this()
        {
            Text = text;
            CallbackName = callbackName;
            IsCallbackFunction = false;
        }

        public ButtonAttribute()
        {
            IsCallbackFunction = true;
        }
    }
}
