using System;
using System.Linq;
using System.Reflection;
using Treasured.UnitySdk;

namespace Treasured
{
    public abstract class NodeGraph : XNode.NodeGraph
    {
        protected virtual void OnEnable()
        {
            // Remove null node references
            foreach (var node in nodes)
            {
                if (node.IsNullOrNone())
                    this.nodes.Remove(node);
            }
        }

        public XNode.Node GetNodeOfType<T>() where T : XNode.Node
        {
            return GetNodeOfType(typeof(T));
        }

        public XNode.Node GetNodeOfType(Type type)
        {
            return nodes.FirstOrDefault(t => t.GetType() == type);
        }
    }
}
