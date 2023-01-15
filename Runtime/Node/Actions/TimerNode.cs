using System;
using Treasured.UnitySdk;
using UnityEngine;

namespace Treasured.Actions
{
    [API("timer")]
    public class TimerNode : ActionNode
    {
        [Min(1)]
        public int duration = 1000;
    }
}
