using System;
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
    }
}
