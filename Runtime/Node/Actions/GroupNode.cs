using System;

namespace Treasured.Actions
{
    [NodeWidth(160)]
    public class GroupNode : ActionNode
    {
        [Output]
        public ActionNode actions;
    }
}
