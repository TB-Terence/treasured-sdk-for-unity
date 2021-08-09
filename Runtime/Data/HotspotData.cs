using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Treasured.ExhibitX
{
    [Serializable]
    public sealed class HotspotData
    {
        public readonly string guid;
        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 position;
        public List<InteractionData> interactions = new List<InteractionData>();

        [JsonConstructor]
        private HotspotData(string guid)
        {
            this.guid = guid;
        }

        public HotspotData(Vector3 position, IEnumerable<InteractionData> interactions) : this(Guid.NewGuid().ToString())
        {
            this.position = position;
            this.interactions = interactions.ToList();
        }
    }
}
