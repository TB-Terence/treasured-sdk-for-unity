using System;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [Serializable]
    public abstract class ActionBase
    {
        [SerializeField]
        [HideInInspector]
        private string _id = Guid.NewGuid().ToString();

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
