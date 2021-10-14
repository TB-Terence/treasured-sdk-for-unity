using System;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [Serializable]
    public abstract class ActionBase
    {
        [SerializeField]
        [HideInInspector]
        [GUID]
        private string _id = Guid.NewGuid().ToString();

        public string Id { get => _id; }

        [SerializeField]
        [Tooltip("Action executing order. When two actions has the same priority, both will be executed at the same time.")]
        private int _priority;

        public int Priority => _priority;

        public string Type {
            get
            {
                string name = this.GetType().Name;
                if (name.EndsWith("Action") && name.Length > 6)
                {
                    name = name.Substring(0, name.Length - 6);
                }
                if(name.Length > 1)
                {
                    name = char.ToLower(name[0]) + name.Substring(1);
                }
                return name;
            }
        }
    }
}
