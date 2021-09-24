using Newtonsoft.Json;
using UnityEngine;

namespace Treasured.UnitySdk
{
    public class SelectObjectAction : ActionBase
    {
        [SerializeField]
        private TreasuredObject _target;

        [JsonIgnore]
        public TreasuredObject Target => _target;

        /// <summary>
        /// Id of the object to select.
        /// </summary>
        [JsonProperty]
        private string targetId => _target ? _target.Id : string.Empty;
    }
}
