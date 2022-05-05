using Newtonsoft.Json;
using System;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [Serializable]
    public class FloatingIcon
    {
        [JsonProperty("icon")]
        public IconAsset asset;
        /// <summary>
        /// Transform of the button.
        /// </summary>
        public Transform transform;
    }
}
