using System;
using Treasured.UnitySdk;

namespace Treasured.Actions
{
    [API("showPreview")]
    public class ShowPreviewNode : ActionNode
    {
        public TreasuredObject target;
    }
}
