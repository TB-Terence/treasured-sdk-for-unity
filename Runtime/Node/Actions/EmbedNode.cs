using Treasured.UnitySdk;

namespace Treasured.Actions
{
    [API("embed")]
    public class EmbedNode : ActionNode
    {
        [Url]
        public string src;
        public EmbedPosition embedPosition;
    }
}
