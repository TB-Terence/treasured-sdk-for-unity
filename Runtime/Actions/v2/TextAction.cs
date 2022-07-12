using System;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [Category("v2")]
    [API("text")]
    public class TextAction : Action
    {
        [TextArea(3, 0)]
        public string content;
        [Min(1000)]
        public int duration = 3000;

        public override object[] GetArguments()
        {
            return new object[] { content, duration };
        }
    }
}
