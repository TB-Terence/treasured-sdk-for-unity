using Newtonsoft.Json;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [Serializable]
    public abstract class ScriptableAction
    {
        [SerializeField]
        [HideInInspector]
        [GUID]
        private string _id = Guid.NewGuid().ToString();
        [JsonIgnore]
        public string Id { internal set { _id = value; } get => _id; }

        [JsonIgnore]
        [HideInInspector]
        public bool enabled = true;

        /// <summary>
        /// Actual type of the action in string format without Action suffix.
        /// </summary>
        [JsonIgnore]
        public string Type
        {
            get
            {
                APIAttribute apiAttribute = GetType().GetCustomAttributes<APIAttribute>().FirstOrDefault();
                if (apiAttribute != null)
                {
                    return apiAttribute.FunctionName;
                }
                string name = this.GetType().Name;
                if (name.EndsWith("Action") && name.Length > 6)
                {
                    name = name.Substring(0, name.Length - 6);
                }
                if (name.Length > 1)
                {
                    name = char.ToLower(name[0]) + name.Substring(1);
                }
                return name;
            }
        }
    }
}
