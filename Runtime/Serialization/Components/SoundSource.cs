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

        public bool Loop = false;
    }
}
