using UnityEngine;

namespace Treasured.UnitySdk
{
    public class PresetAttribute : PropertyAttribute
    {
        public string[] Values { get; set; } = new string[] { string.Empty };

        public PresetAttribute(params string[] values)
        {
            Values = values;
        }
    }
}
