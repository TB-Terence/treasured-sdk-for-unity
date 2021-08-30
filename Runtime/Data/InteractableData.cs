using Newtonsoft.Json;
using System;

namespace Treasured.UnitySdk
{
    [Serializable]
    public sealed class InteractableData : TreasuredObjectData
    {
        [JsonConstructor]
        private InteractableData(string id) : base(id)
        {

        }
    }
}
