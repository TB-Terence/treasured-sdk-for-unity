using Newtonsoft.Json;
using System;
using UnityEngine;

namespace Treasured.ExhibitX
{
    public sealed class LookAtInteraction : InteractionData
    {
        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 targetPosition;
    }
}
