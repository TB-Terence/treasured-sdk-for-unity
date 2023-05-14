using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;

namespace Treasured.UnitySdk
{
    [AddComponentMenu("Treasured/Sound Source")]
    public class SoundSource : TreasuredObject
    {
        [JsonIgnore]
        [Url]
        [FormerlySerializedAs("Src")]
        public string src;

        [JsonIgnore]
        [Range(0, 100)]
        [FormerlySerializedAs("Volume")]
        public int volume = 100;

        [JsonIgnore]
        [FormerlySerializedAs("Loop")]
        public bool loop = true;

        public AudioInfo audioInfo;

        [JsonProperty("src")]
        string Src
        {
            get
            {
                return audioInfo.Uri;
            }
        }

        [JsonProperty("volume")]
        int Volume
        {
            get
            {
                return audioInfo.volume;
            }
        }

        [JsonProperty("loop")]
        bool Loop
        {
            get
            {
                return audioInfo.loop;
            }
        }

        [FormerlySerializedAs("Distance")]
        [Range(0, 100)]
        public int distance = 3;
    }
}
