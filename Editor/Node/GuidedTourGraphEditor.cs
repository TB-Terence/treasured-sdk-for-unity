using System;
using System.Linq;
using Treasured.Actions;
using Treasured.UnitySdk;
using UnityEditor;
using UnityEngine;
using static XNode.NodeGraph;

namespace Treasured.Actions
{
    [CustomNodeGraphEditor(typeof(GuidedTour.GuidedTourGraph))]
    public class GuidedTourGraphEditor : XNodeEditor.NodeGraphEditor
    {
        private GuidedTour.GuidedTourGraph graph;

        public override void OnOpen()
        {
            graph = serializedObject.targetObject as GuidedTour.GuidedTourGraph;
            CreateTourNodes();
            // Work around for create node menu for choice node
            XNodeEditor.NodeEditorPreferences.GetSettings().createFilter = false;
        }

        public override bool CanRemove(XNode.Node node)
        {
            var infoNode = node as GuidedTour.GuidedTourInfoNode;
            if (infoNode && XNodeEditor.NodeEditorUtilities.GetCachedAttrib<GuidedTour.GuidedTourGraph.CreateTourAttribute>(graph.Owner?.GetType(), nameof(TreasuredMap.guidedTourGraph), out var attribute))
            {
                bool canRemove = !attribute.TourNames.Contains(infoNode.title);
                if(!canRemove)
                    Debug.LogError($"Removing '{infoNode.title}' tour is prohibited.");
                return canRemove;
            }
            return base.CanRemove(node);
        }

        public override string GetNodeMenuName(System.Type type)
        {
            // Hide the options to create event nodes
            if (!type.Namespace.StartsWith("Treasured"))
            {
                return null;
            }
            return base.GetNodeMenuName(type);
        }

        void CreateTourNodes()
        {
            if(XNodeEditor.NodeEditorUtilities.GetCachedAttrib<GuidedTour.GuidedTourGraph.CreateTourAttribute>(graph.Owner?.GetType(), nameof(TreasuredMap.guidedTourGraph), out var attribute))
            {
                foreach (var name in attribute.TourNames)
                {
                    if(graph.nodes.OfType<GuidedTour.GuidedTourInfoNode>().Any(x => x.title == name))
                    {
                        continue;
                    }
                    var node = graph.AddNode<Treasured.GuidedTour.GuidedTourInfoNode>();
                    node.title = name;
                    node.position.y += 200;
                }
            }
        }
    }
}
