using System;
using UnityEngine;

namespace Treasured.UnitySdk
{
    /// <summary>
    /// Base class for action
    /// </summary>
    [Serializable]
    public abstract class Action
    {
        [SerializeField]
        [HideInInspector]
        [GUID]
        private string _id = Guid.NewGuid().ToString();

        public string Id { get => _id; }

        /// <summary>
        /// Actual type of the action in string format without Action suffix.
        /// </summary>
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
