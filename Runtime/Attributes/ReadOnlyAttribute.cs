using UnityEngine;

namespace Treasured.SDK
{
    public class ReadOnlyAttribute : PropertyAttribute
    {
        public ReadOnlyAttribute()
        {
            order = 99999;
        }
    }
}
