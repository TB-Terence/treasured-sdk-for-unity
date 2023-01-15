using System;
using Treasured.Actions;
using UnityEngine;
using XNode;

namespace Treasured.GuidedTour
{
    public class GuidedTourInfoNode : XNode.Node
    {
        [SerializeField]
        [HideInInspector]
        private string _id = Guid.NewGuid().ToString();
        public string Id { get { return _id; } }
        public string title = "New Tour";
        [TextArea(3, 5)]
        public string description = "";
        [TextArea(3, 5)]
        public string thumbnailUrl = "";
        [Output]
        public ActionNode onTourStart;

        public override object GetValue(NodePort port)
        {
            if (port == null)
            {
                return null;
            }
            if (port.fieldName == nameof(onTourStart))
            {
                return port.Connection?.node;
            }
            return null;
        }
    }
}
