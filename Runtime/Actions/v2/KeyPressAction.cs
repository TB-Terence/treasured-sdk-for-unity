using System;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [Category("v2")]
    [API("keyPress")]
    public class KeyPressAction : Action
    {
        public KeyCode keyCode;

        public override object[] GetArguments()
        {
            return new object[] { Enum.GetName(typeof(KeyCode), keyCode) };
        }
    }
}
