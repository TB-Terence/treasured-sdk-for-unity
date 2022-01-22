using System;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property |
        AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
    public class ShowIfAttribute : PropertyAttribute
    {
        public string ConditionalField = "";
        public string ValueToCompare = "";

        /// <summary>
        /// Show the field if the string field matches with the provided condition
        /// </summary>
        /// <param name="conditionalField">The name of the string field that will be in control</param>
        /// <param name="valueToCompare">Compare with the string value</param>
        public ShowIfAttribute(string conditionalField, string valueToCompare)
        {
            this.ConditionalField = conditionalField.ToLower();
            this.ValueToCompare = valueToCompare.ToLower();
        }
    }
}