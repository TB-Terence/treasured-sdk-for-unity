using UnityEngine;
using System;
using Newtonsoft.Json;

namespace Treasured.ExhibitX
{
    [Serializable]
    [JsonConverter(typeof(InteractionDataConverter))]
    public abstract class InteractionData : ScriptableObject
    {
        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 position;
    }
}
