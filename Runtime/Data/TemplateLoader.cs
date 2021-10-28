using System;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [Serializable]
    public class TemplateLoader
    {
        [Preset("simple", "standard", "modern")]
        public string template;
        [TextArea(3, 3)]
        public string imageUrl;
    }
}