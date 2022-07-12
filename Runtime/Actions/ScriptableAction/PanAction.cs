using UnityEngine;

namespace Treasured.UnitySdk
{
    [API("pan")]
    public class PanAction : ScriptableAction
    {
        public float amount = 0.1f;
        [Min(1)]
        public int duration = 1000;

        public override object[] GetArguments()
        {
            return new object[] { amount, duration };
        }
    }
}
