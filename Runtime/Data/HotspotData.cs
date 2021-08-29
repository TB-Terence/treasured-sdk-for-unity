using Newtonsoft.Json;
using Treasured.SDK;

namespace Treasured.UnitySdk
{
    public sealed class HotspotData : TreasuredObjectData
    {
        private HotspotData(Hotspot hotspot) : base(hotspot)
        {

        }

        [JsonConstructor]
        private HotspotData(string id) : base(id)
        {

        }

        public static implicit operator HotspotData(Hotspot data)
        {
            return new HotspotData(data);
        }
    }
}
