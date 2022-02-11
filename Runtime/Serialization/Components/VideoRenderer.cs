using Newtonsoft.Json;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [AddComponentMenu("Treasured/Video Renderer")]
    public class VideoRenderer : TreasuredObject
    {
        #region data
        [SerializeField]
        private bool _lockAspectRatio;
        [SerializeField]
        private string _aspectRatio = "16:9";
        [Url]
        public string Src;
        [Range(0, 100)]
        public int Volume = 100;
        public bool Loop = true;
        #endregion

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
