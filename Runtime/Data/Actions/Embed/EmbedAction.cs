using UnityEngine;

namespace Treasured.UnitySdk
{
    [Category("Embed")]
    public abstract class EmbedAction : Action
    {
        [SerializeField]
        private EmbedPosition _position = EmbedPosition.TopRight;

        public EmbedPosition Position { get => _position; set => _position = value; }
    }
}
