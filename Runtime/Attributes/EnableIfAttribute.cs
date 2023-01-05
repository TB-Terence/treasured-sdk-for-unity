using System;
using UnityEngine;

namespace Treasured.UnitySdk
{
    /// <summary>
    /// EnableIf is used for enable or disable the property in the inspector if field is not null.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property |
                    AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = true)]
    public class EnableIfAttribute : PropertyAttribute
    {
        public string ConditionalField = "";

        public EnableIfAttribute(string conditionalField)
        {
            ConditionalField = conditionalField;
        }
    }
}
