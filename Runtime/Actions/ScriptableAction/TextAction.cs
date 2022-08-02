using UnityEngine;

namespace Treasured.UnitySdk
{
    [API("text")]
    public class TextAction : ScriptableAction
    {
        [TextArea(3, 0)]
        public string message;

        public override object[] GetArguments()
        {
            return new object[] { message };
        }
    }
}
