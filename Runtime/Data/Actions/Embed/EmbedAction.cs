using Newtonsoft.Json;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [Category("Media")]
    public abstract class EmbedAction : ActionBase
    {
        [SerializeField]
        [TextArea(1, 3)]
        private string _src;

        [SerializeField]
        private EmbedPosition _position = EmbedPosition.TopRight;

        public string Src { get => _src; set => _src = value; }

        [JsonProperty("displayMode")]
        public EmbedPosition Position { get => _position; set => _position = value; }
    }
}
