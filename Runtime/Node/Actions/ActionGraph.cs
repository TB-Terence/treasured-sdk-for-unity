using System.Linq;
using Treasured.Events;
using Treasured.UnitySdk;
using UnityEngine;

namespace Treasured.Actions
{
    [RequiredEventNode(typeof(Hotspot), typeof(OnSelectNode))]
    [RequiredEventNode(typeof(Interactable), typeof(OnClickNode), typeof(OnHoverNode))]
    public class ActionGraph : NodeGraph
    {
        [SerializeField]
        [HideInInspector]
        private UnityEngine.Object _owner;

        public UnityEngine.Object Owner { get { return _owner; } internal set { _owner = value; } }

        public static ActionGraph Create(UnityEngine.Object owner)
        {
            ActionGraph graph = ScriptableObject.CreateInstance<ActionGraph>();
            graph._owner = owner;
            return graph;
        }
    }
}
