using Newtonsoft.Json;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [AddComponentMenu("Treasured/HTML Embed")]
    public class HTMLEmbed : TreasuredObject
    {
        [SerializeField]
        private bool _lockAspectRatio;
        [SerializeField]
        private string _aspectRatio = "16:9";
        [Url]
        public string src;

        [JsonIgnore]
        public bool LockAspectRatio => _lockAspectRatio;
        [JsonIgnore]
        public float AspectRatio
        {
            get
            {
                string[] ratios = _aspectRatio.Split(':', 'x');
                return ratios.Length == 2 ? float.Parse(ratios[0]) / float.Parse(ratios[1]) : 1;
            }
        }
    }
}
