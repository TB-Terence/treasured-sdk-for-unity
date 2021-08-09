using UnityEngine;

namespace Treasured.ExhibitX
{
    public sealed class ShowTextInteraction : InteractionData
    {
        [TextArea(3, 3)]
        public string content;

        private ShowTextInteraction() { }
    }
}
