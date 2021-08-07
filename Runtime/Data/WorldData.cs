using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace Treasured.ExhibitX
{
    [Serializable]
    public class WorldData
    {
        public string name;
        [JsonConverter(typeof(StringEnumConverter))]
        public ImageQuality quality;
        [JsonConverter(typeof(StringEnumConverter))]
        public ImageFormat format;
        [JsonConverter(typeof(VersionConverter))]
        public Version version = new Version(1, 0, 0);
        public bool loop;
        public List<HotspotData> hotspots = new List<HotspotData>();
    }
}
