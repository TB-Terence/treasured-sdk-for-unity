using System;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [API("keyPress")]
    public class KeyPressAction : ScriptableAction
    {
        public KeyCode keyCode;

        public override object[] GetArguments()
        {
            return new object[] { Enum.GetName(typeof(KeyCode), keyCode) };
        }
    }
}
