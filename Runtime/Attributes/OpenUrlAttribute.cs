using UnityEngine;

namespace Treasured.UnitySdk
{
    internal class OpenUrlAttribute : PropertyAttribute
    {
        public string Url { get; }

        public OpenUrlAttribute(string url)
        {
            Url = url;
        }
    }
}
