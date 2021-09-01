using Newtonsoft.Json;
using System;

namespace Treasured.UnitySdk
{
    [Serializable]
    public sealed class HotspotData : TreasuredObjectData
    {
        [JsonConstructor]
        private HotspotData(string id) : base(id)
        {

        }

        internal HotspotData() : base()
        {

        }
    }
}
