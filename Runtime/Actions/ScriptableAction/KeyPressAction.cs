using System;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [API("button")]
    [Obsolete]
    public class KeyPressAction : ScriptableAction
    {
        public KeyCode key;
    }
}
