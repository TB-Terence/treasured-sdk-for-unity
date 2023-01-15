using System.Linq;
using Treasured.UnitySdk;
using UnityEditor;
using UnityEngine;

namespace Treasured.Actions
{
    [CustomNodeGraphEditor(typeof(ActionGraph))]
    public class ActionGraphEditor : XNodeEditor.NodeGraphEditor
    {
        private ActionGraph graph;
        private TreasuredObject owner;
        private SerializedProperty serializedProperty;

        public override void OnOpen()
        {
            graph = serializedObject.targetObject as ActionGraph;
            owner = (TreasuredObject)graph.Owner;
            //this.window.titleContent = new GUIContent($"{base.window.titleContent.text}/Action Graph - {graph.name}");
            CreateEventNodes();
            // Work around for create node menu for choice node
            XNodeEditor.NodeEditorPreferences.GetSettings().createFilter = false;
        }

        public override bool CanRemove(XNode.Node node)
        {
            var eventNodeTypes = ReflectionUtils.GetAttributes<RequiredEventNodeAttribute>(graph.GetType()).SelectMany(x => x.Types);
            if (eventNodeTypes.Any(t => t == node.GetType()))
            {
                Debug.LogError($"Removing {node.GetType().Name} is prohibited.");
                return false;
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

        void CreateEventNodes()
        {
            var attributes = ReflectionUtils.GetAttributes<RequiredEventNodeAttribute>(graph.GetType());
            if(attributes == null)
            {
                return;
            }
            foreach(var attribute in attributes)
            {
                if(owner.GetType() != attribute.OwnerType)
                {
                    continue;
                }
                for (int i = 0; i < attribute.Types.Length; i++)
                {
                    var type = attribute.Types[i];
                    var node = graph.nodes.FirstOrDefault(x => x.GetType() == type);
                    if (node.IsNullOrNone())
                    {
                        var newNode = graph.AddNode(type);
                        newNode.name = ObjectNames.NicifyVariableName(type.Name.Replace("Node", ""));
                        if (newNode.position == Vector2.zero)
                            newNode.position = new Vector2(0, i * 100);
                    }
                }
            }
        }
    }
}
