using System.Linq;
using Treasured.Events;
using Treasured.UnitySdk;
using UnityEngine;

namespace Treasured.Actions
{
    [CreateEventNode(typeof(Hotspot), typeof(OnSelectNode))]
    [CreateEventNode(typeof(Interactable), typeof(OnClickNode), typeof(OnHoverNode))]
    [RequireNode(typeof(OnSelectNode))]
    public class ActionGraph : NodeGraph
    {
        [SerializeField]
        [HideInInspector]
        private UnityEngine.Object owner;

        public UnityEngine.Object Owner { get { return owner; } private set { owner = value; } }

        public static ActionGraph Create(UnityEngine.Object owner)
        {
            ActionGraph graph = ScriptableObject.CreateInstance<ActionGraph>();
            graph.owner = owner;
            return graph;
        }
    }
}
