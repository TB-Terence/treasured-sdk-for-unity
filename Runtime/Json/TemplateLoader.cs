using System;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [Serializable]
    public class TemplateLoader
    {
        [Preset("simple", "standard", "modern")]
        public string template;

        [ShowIf("template", "modern")]
        public bool autoCameraRotation;

        [TextArea(3, 3)]
        [RequiredField]
        public string imageUrl;
    }
}