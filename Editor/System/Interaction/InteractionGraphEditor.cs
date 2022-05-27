using System;
using XNode;
using XNodeEditor;

namespace Treasured.UnitySdk.Interaction
{
    [CustomNodeGraphEditor(typeof(InteractionGraph))]
    public class InteractionGraphEditor : NodeGraphEditor
    {
        private const string NamespacePrefix = "Treasured.UnitySdk";
        private const string NamespaceMenuNamePrefix = "Treasured/Unity Sdk/Interaction";

        /// <summary>
        /// Sort out nodes in the TreasureWorld.CustomGraph namespace for the Context Menu
        /// </summary>
        public override string GetNodeMenuName(Type type)
        {
            if (type.Namespace.StartsWith(NamespacePrefix))
            {
                return base.GetNodeMenuName(type).Replace(NamespaceMenuNamePrefix, "Treasured");
            }
            return null;
        }

        public override void OnCreate()
        {
            base.OnCreate();
        }
    }
}
