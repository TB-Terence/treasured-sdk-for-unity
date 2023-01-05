using System;
using UnityEngine;

namespace Treasured.UnitySdk
{
    /// <summary>
    /// OnValueChanged attribute calls the specified function whenever the reference value has been changed via the inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class OnValueChangedAttribute : PropertyAttribute
    {
        public string CallbackName { get; private set; }

        public OnValueChangedAttribute(string callbackName)
        {
            CallbackName = callbackName;
        }
    }
}
