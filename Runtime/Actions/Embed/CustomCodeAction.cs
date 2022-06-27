using Newtonsoft.Json;
using UnityEngine.Serialization;

namespace Treasured.UnitySdk
{
    /// <summary>
    /// Action that runs a custom embedded code.
    /// </summary>
    public class CustomCodeAction : EmbedAction
    {
        [JsonIgnore]
        [Code]
        [FormerlySerializedAs("_code")]
        public string code;
    }
}