using UnityEngine;

namespace Treasured.UnitySdk
{
    [API("pan")]
    public class PanAction : ScriptableAction
    {
        public float amount = 0.1f;

        public override object[] GetArguments()
        {
            return new object[] { amount };
        }
    }
}
