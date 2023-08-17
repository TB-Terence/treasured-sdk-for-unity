using System;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [Serializable]
    public class TemplateLoader
    {
        [RequiredField]
        [Preset("minimal", "simple", "standard", "modern")]
        public string template = "minimal";
        [ShowIf(nameof(ShowAutoCameraRotation))]
        public bool autoCameraRotation;

        [TextArea(3, 3)]
        public string imageUrl;

        bool ShowAutoCameraRotation()
        {
            return !string.IsNullOrEmpty(template) && template.Equals("modern");
        }
    }
}