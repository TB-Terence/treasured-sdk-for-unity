using UnityEngine;
using XNode;

namespace Treasured.UnitySdk.Interaction
{
    public class InteractionNode : Node
    {
        [Input(ShowBackingValue.Never)]
        public Node previous;
        [Output(ShowBackingValue.Never)]
        public Node next;

        protected override void Init()
        {
        }
    }
}
