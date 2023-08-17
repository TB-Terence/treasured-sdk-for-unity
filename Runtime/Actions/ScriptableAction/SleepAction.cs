using Newtonsoft.Json;
using System;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [API("sleep")]
    public class SleepAction : ScriptableAction
    {
        [Tooltip("Duration in seconds")]
        [Min(0)]
        [JsonIgnore]
        public float duration = 1;

        /// <summary>
        /// Duration in milliseconds
        /// </summary>
        [JsonProperty]
        public float Duration
        {
            get
            {
                return duration * 1000;
            }
        }
    }
}
