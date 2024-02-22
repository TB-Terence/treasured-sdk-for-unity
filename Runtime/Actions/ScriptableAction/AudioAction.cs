using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [API("playAudio")]
    public class AudioAction : ScriptableAction
    {
        [HideInInspector]
        [TextArea(3, 5)]
        public string src;

        public AudioInfo audioInfo;

        //[Range(0, 100)]
        //public int volume = 100;

        //public bool loop = false;
        [TextArea(3, 5)]
        public string message = "";
        
        public bool allowAutoStop = true;
        
        [ShowIf("allowAutoStop")]
        [Tooltip("Auto stop audio duration in seconds")]
        [Min(0)]
        [JsonIgnore]
        public int autoStopDuration = 10;

        /// <summary>
        /// Auto Close Duration in milliseconds
        /// </summary>
        [JsonProperty]
        public float AutoStopDuration
        {
            get
            {
                return autoStopDuration * 1000;
            }
        }
    }
}
