﻿using Newtonsoft.Json;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Serialization;
using Treasured.UnitySdk;

namespace Treasured.Actions
{
    [API("text")]
    public class ShowTextNode : ActionNode
    {
        public const string kPattern = "[^\\w]";
        public const int kAverageWordsReadPerSecond = 3;

        [TextArea(3, 3)]
        public string message;

        [JsonIgnore]
        [SerializeField]
        [Min(0)]
        [Tooltip("Text display duration in seconds.")]
        [FormerlySerializedAs("_duration")]
        public int duration;

        /// <summary>
        /// Modified duration based on message in milliseconds.
        /// </summary>
        public int Duration
        {
            get
            {
                if (duration == 0)
                {
                    return Mathf.Max(1, Regex.Split(message, kPattern, RegexOptions.IgnoreCase).Length / kAverageWordsReadPerSecond) * 1000;
                }
                else
                {
                    return duration * 1000;
                }
            }
        }
    }
}
