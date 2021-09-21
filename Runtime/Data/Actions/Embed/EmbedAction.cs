using UnityEngine;

namespace Treasured.UnitySdk
{
    [Category("Media")]
    public abstract class EmbedAction : ActionBase
    {
        [TextArea(1, 3)]
        public string src;

        public EmbedPosition position = EmbedPosition.TopRight;
    }
}
