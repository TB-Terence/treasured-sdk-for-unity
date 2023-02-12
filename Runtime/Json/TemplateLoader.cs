using System;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [Serializable]
    public class TemplateLoader
    {
        [RequiredField]
        [Preset("minimal", "simple", "standard", "modern")]
        public string template;
        [ShowIf("EnableAutoCameraRotation")]
        public bool autoCameraRotation;

        [RequiredField]
        [TextArea(3, 3)]
        public string imageUrl;

        bool EnableAutoCameraRotation()
        {
            return template.Equals("modern");
        }
    }
}