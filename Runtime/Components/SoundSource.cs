using Newtonsoft.Json;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [AddComponentMenu("Treasured/Sound Source")]
    public class SoundSource : TreasuredObject
    {
        [Url]
        public string Src;

        [Range(0, 100)]
        public int Volume = 100;

        public bool Loop = true;

        [Range(0, 100)]
        public int Distance = 3;

        public AudioContent audioContent;
    }
}
