using System;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [Category("v2")]
    [API("sleep")]
    public class SleepAction : Action
    {
        [Min(1)]
        public int duration;

        public override object[] GetArguments()
        {
            return new object[] { duration };
        }
    }
}
