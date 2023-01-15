using Treasured.Actions;
using XNode;

namespace Treasured.Events
{
    [NodeWidth(120)]
    [NodeTint("#008631")]
    public abstract class EventNode : Node
    {
        [Output(ShowBackingValue.Never, ConnectionType.Override)]
        public ActionNode action;

        public override object GetValue(NodePort port)
        {
            if (port == null)
            {
                return null;
            }
            if (port.fieldName == nameof(action))
            {
                return port.Connection?.GetOutputValue();
            }
            return null;
        }
    }
}
