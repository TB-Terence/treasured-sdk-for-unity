using Newtonsoft.Json;
using System;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [Serializable]
    public class InteractableButton
    {
        [JsonProperty("icon")]
        public IconAsset asset;
        /// <summary>
        /// Transform of the button.
        /// </summary>
        public Transform transform;
        public ButtonPreview preview;
    }
}
