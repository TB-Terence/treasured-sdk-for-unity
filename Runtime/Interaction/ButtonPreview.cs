using System;
using UnityEngine;
namespace Treasured.UnitySdk
{
    [Serializable]
    public class ButtonPreview
    {
        public string title;
        public string subtitle;
        [TextArea(3, 5)]
        public string description;
        [Url]
        public string src;
    }
}
