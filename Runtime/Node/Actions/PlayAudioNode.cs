using Treasured.UnitySdk;
using UnityEngine;

namespace Treasured.Actions
{
    [API("playAudio")]
    public class PlayAudioNode : ActionNode
    {
        [TextArea(3, 5)]
        public string src;

        [Range(0, 100)]
        public int volume = 100;

        public bool loop = false;
        [TextArea(3, 5)]
        public string message;
    }
}
