using UnityEngine;

namespace Treasured.UnitySdk
{
    public sealed class SetBackgroundMusic : Action
    {
        [TextArea(3, 8)]
        public string src;
        [Range(0, 100)]
        public int volume = 50;
    }
}
