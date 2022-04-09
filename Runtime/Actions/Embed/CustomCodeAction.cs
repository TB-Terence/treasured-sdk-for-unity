using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;

namespace Treasured.UnitySdk
{
    /// <summary>
    /// Action that runs a custom embedded code.
    /// </summary>
    public class CustomCodeAction : EmbedAction
    {
        [SerializeField]
        //[TextArea(9, 9)]
        [Code]
        [FormerlySerializedAs("_src")]
        private string _code;
        
        [System.Obsolete("Use Code Property instead")]
        public string Src { get => _code; set => _code = value; }

        [JsonIgnore]
        public string Code { get => _code; set => _code = value; }
    }
}