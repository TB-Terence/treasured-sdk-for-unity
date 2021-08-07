using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Treasured.ExhibitX
{
    [Serializable]
    public class HotspotData
    {
        public string guid;
        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 position;
        public List<InteractionData> interactions = new List<InteractionData>();
    }
}
