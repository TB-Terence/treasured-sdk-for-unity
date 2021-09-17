using UnityEngine;

namespace Treasured.UnitySdk
{
    public class ReadOnlyAttribute : PropertyAttribute
    {
        public ReadOnlyAttribute()
        {
            order = 99999;
        }
    }
}
