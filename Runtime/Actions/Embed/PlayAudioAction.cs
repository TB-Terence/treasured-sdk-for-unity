using UnityEngine;
using UnityEngine.Serialization;

namespace Treasured.UnitySdk
{
    /// <summary>
    /// Action that plays audio from a source.
    /// </summary>
    public class PlayAudioAction : EmbedAction
    {
        [Url]
        [FormerlySerializedAs("_src")]
        public string src;

        [Range(0, 100)]
        [FormerlySerializedAs("_volume")]
        public int volume = 100;
    }
}
