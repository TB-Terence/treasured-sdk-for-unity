using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

namespace Treasured.ExhibitX
{
    public enum Alignment
    {
        Center,
        Left,
        Right,
        FullScreen
    }

    public sealed class OpenLinkInteraction : InteractionData
    {
        [TextArea(3, 3)]
        public string url;
        [JsonConverter(typeof(StringEnumConverter))]
        public Alignment alignment;
    }
}
