using Newtonsoft.Json;
using UnityEngine;

namespace Treasured.UnitySdk
{
    /// <summary>
    /// Action that executes actions on another object.
    /// </summary>
    public class SelectObjectAction : Action
    {
        [JsonIgnore]
        [SerializeField]
        public TreasuredObject target;

        /// <summary>
        /// Id of the object to select.
        /// </summary>
        [JsonProperty]
        private string targetId => target ? target.Id : string.Empty;

        [JsonProperty]
        private string targetType
        {
            get
            {
                string typeName = target.GetType().Name;
                return target.IsNullOrNone() ? string.Empty : char.ToLower(typeName[0]) + typeName.Substring(1);
            }
        }
    }
}
