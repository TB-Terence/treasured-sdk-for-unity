using System;
using UnityEngine;

namespace Treasured.UnitySdk
{
    /// <summary>
    /// EnableIf is used for enable or disable the property in the inspector if field is not null.
    /// </summary>
    public class EnableIfAttribute : PropertyAttribute
    {
        public string Getter { get; private set; }

        public EnableIfAttribute(string getter)
        {
            Getter = getter;
        }
    }

    public class ShowIfAttribute : PropertyAttribute
    {
        public string Getter { get; private set; }

        public ShowIfAttribute(string getter)
        {
            Getter = getter;
        }
    }

    public class ReadOnlyAttribute : PropertyAttribute { }
    public class RequiredFieldAttribute : PropertyAttribute
    {
        public string Text { get; private set; }
        public RequiredFieldAttribute(string text)
        {
            this.Text = text;
        }

        public RequiredFieldAttribute()
        {
            this.Text = "This field is required.";
        }
    }

    public class PresetAttribute : PropertyAttribute
    {
        public bool Customizable { get; } = false;
        public string[] Values { get; private set; }
        public PresetAttribute(params string[] values) : this (true, values)
        {
        }

        public PresetAttribute(bool customizable, params string[] values)
        {
            this.Customizable = customizable;
            Values = values;
        }
    }

    /// <summary>
    /// OnValueChanged attribute calls the specified function whenever the reference value has been changed via the inspector.
    /// </summary>
    public class OnValueChangedAttribute : PropertyAttribute, IMethodInvoker
    {
        public string CallbackName { get; private set; }

        public OnValueChangedAttribute(string callbackName)
        {
            CallbackName = callbackName;
        }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field, AllowMultiple = true)]
    public class ButtonAttribute : PropertyAttribute, IMethodInvoker
    {
        public string Text { get; private set; }
        public string CallbackName { get; private set; }
        internal bool HasCallback { get; private set; } = false;

        /// <summary>
        /// </summary>
        /// <param name="text"></param>
        public ButtonAttribute(string text) : this(text, string.Empty)
        {
            Text = text;
            HasCallback = true;
        }

        /// <summary>
        /// </summary>
        /// <param name="text">Text for the button</param>
        /// <param name="callbackName">Name of zero parameter method</param>
        public ButtonAttribute(string text, string callbackName)
        {
            Text = text;
            CallbackName = callbackName;
            HasCallback = !string.IsNullOrEmpty(callbackName);
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class LabelAttribute : PropertyAttribute
    {
        public string Text { get; private set; }
        public LabelAttribute(string text)
        {
            Text = text;
        }
    }

    public interface IMethodInvoker
    {
        public string CallbackName { get; }
    }
}
