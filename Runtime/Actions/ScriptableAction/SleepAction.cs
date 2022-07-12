﻿using System;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [API("sleep")]
    public class SleepAction : ScriptableAction
    {
        [Min(1)]
        public int duration;

        public override object[] GetArguments()
        {
            return new object[] { duration };
        }
    }
}