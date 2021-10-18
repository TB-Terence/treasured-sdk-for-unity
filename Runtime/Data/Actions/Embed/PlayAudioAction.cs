using UnityEngine;

namespace Treasured.UnitySdk
{
    /// <summary>
    /// Action that plays audio from a source.
    /// </summary>
    public class PlayAudioAction : EmbedAction
    {
        [SerializeField]
        [Range(0, 100)]
        private int _volume = 100;

        public int Volume { get => _volume; set => _volume = value; }
    }
}
