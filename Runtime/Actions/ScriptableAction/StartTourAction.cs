using System;

namespace Treasured.UnitySdk
{
    [API("startTour")]
    public class StartTourAction : ScriptableAction
    {
        public GuidedTour target;
    }
}
