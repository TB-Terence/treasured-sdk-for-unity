using System;
using Treasured.UnitySdk;

namespace Treasured.Actions
{
    [API("startTour")]
    public class StartTourNode : ActionNode
    {
        public GuidedTour target;
    }
}
