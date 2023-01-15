using UnityEngine;
using Treasured.UnitySdk;

namespace Treasured.Actions
{
    [API("button")]
    public class ButtonNode : ActionNode
    {
        [TextArea]
        public string text;
        [Output]
        public ActionNode yes;
        [Output]
        public ActionNode no;
    }
}
