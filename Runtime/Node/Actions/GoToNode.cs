using Treasured.UnitySdk;
using UnityEngine;

namespace Treasured.Actions
{
    [API("goto")]
    [CreateActionGroup(typeof(SetCameraRotationNode))]
    [CreateActionGroup(typeof(PanNode))]
    public class GoToNode : ActionNode
    {
        public Hotspot target;
        [TextArea]
        public string message;
    }
}
