using UnityEngine;
using UnityEngine.Serialization;

namespace Treasured.UnitySdk
{
    [Category("Embed")]
    public abstract class EmbedAction : Action
    {
        [FormerlySerializedAs("_position")]
        public EmbedPosition position = EmbedPosition.TopRight;
    }
}
