using UnityEngine;

namespace Treasured.UnitySdk
{
    [API("embed")]
    public class EmbedAction : ScriptableAction
    {
        [Url]
        public string src;
        public string title;
        [TextArea(3, 5)]
        public string description;
        [Url]
        public string previewUrl;
        public EmbedPosition position = EmbedPosition.Fullscreen;
    }
}
