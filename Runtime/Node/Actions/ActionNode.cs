using Newtonsoft.Json;
using System;
using UnityEngine;
using XNode;

namespace Treasured.Actions
{
    [NodeWidth(240)]
    public abstract class ActionNode : Node
    {
        [SerializeField]
        [HideInInspector]
        private string _id = Guid.NewGuid().ToString();
        public string Id { get { return _id; } }
        [Input(ShowBackingValue.Never)]
        [HideInInspector]
        public ActionNode previous;
        [Output(ShowBackingValue.Never)]
        [HideInInspector]
        public ActionNode next;

        /// <summary>
        /// Actual type of the action in string format without Action suffix.
        /// </summary>
        [JsonIgnore]
        public string Type
        {
            get
            {
                string name = this.GetType().Name;
                if (name.EndsWith("Node") && name.Length > 4)
                {
                    name = name.Substring(0, name.Length - 4);
                }
                if (name.Length > 1)
                {
                    name = char.ToLower(name[0]) + name.Substring(1);
                }
                return name;
            }
        }

        public override object GetValue(NodePort port)
        {
            if(port == null)
            {
                return null;
            }
            if(port.fieldName == nameof(next))
            {
                return port.Connection?.node;
            }
            return null;
        }
    }
}
