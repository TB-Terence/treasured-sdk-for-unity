using System;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [Serializable]
    public class Button
    {
        /// <summary>
        /// Name of the icon for the icon.
        /// </summary>
        public string icon = "FaCircle";
        [Newtonsoft.Json.JsonIgnore]
        public IconAsset iconAsset;
        /// <summary>
        /// Transform of the button.
        /// </summary>
        public Transform transform;
    }
}
