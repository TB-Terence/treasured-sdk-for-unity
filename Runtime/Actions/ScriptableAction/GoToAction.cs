using System;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [API("goto")]
    public class GoToAction : ScriptableAction
    {
        public TreasuredObject target;

        public override object[] GetArguments()
        {
            return new object[] { target?.Id };
        }
    }
}
