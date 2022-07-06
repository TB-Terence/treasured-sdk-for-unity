using System;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [Category("v2")]
    [API("goto")]
    public class GoToAction : Action
    {
        public TreasuredObject target;

        public override object[] GetArguments()
        {
            return new object[] { target?.Id };
        }
    }
}
