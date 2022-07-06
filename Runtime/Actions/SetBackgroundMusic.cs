using UnityEngine;

namespace Treasured.UnitySdk
{
    public sealed class SetBackgroundMusic : Action
    {
        [Url]
        public string src;
        [Range(0, 100)]
        public int volume = 50;
    }
}
