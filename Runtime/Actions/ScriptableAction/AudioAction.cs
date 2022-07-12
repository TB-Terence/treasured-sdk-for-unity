using UnityEngine;

namespace Treasured.UnitySdk
{
    [API("playAudio")]
    public class AudioAction : ScriptableAction
    {
        [Url]
        public string src;

        [Range(0, 100)]
        public int volume = 100;

        public bool loop = false;
    }
}
