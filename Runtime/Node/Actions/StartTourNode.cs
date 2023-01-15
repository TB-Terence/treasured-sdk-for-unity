using Newtonsoft.Json;
using System;
using Treasured.UnitySdk;

namespace Treasured.Actions
{
    [API("startTour")]
    public class StartTourNode : ActionNode
    {
        [JsonIgnore]
        public Treasured.UnitySdk.GuidedTour target;
    }
}
