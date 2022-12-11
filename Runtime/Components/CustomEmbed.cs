using System;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [Serializable]
    public class CustomEmbed
    {
        [TextArea]
        public string head;
        [TextArea]
        public string body;
        public Rect position;
    }
}
