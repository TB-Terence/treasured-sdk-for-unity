using Newtonsoft.Json;
using System;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [AddComponentMenu("Treasured/Interactable")]
    public sealed class Interactable : TreasuredObject
    {
        [Obsolete]
        [SerializeField]
        private InteractableData _data = new InteractableData();

        [Obsolete]
        [JsonIgnore]
        public override TreasuredObjectData Data
        {
            get
            {
                return _data;
            }
        }
    }
}
