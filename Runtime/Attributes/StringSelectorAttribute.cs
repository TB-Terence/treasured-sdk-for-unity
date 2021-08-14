using UnityEngine;

namespace Treasured.SDK
{
    public class StringSelectorAttribute : PropertyAttribute
    {
        public string[] Values { get; set; } = new string[] { string.Empty };

        public StringSelectorAttribute(params string[] values)
        {
            Values = values;
        }
    }
}
