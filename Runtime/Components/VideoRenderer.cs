using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Video;

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

        [JsonIgnore]
        public VideoClip VideoClip;

        [Url]
        [FormerlySerializedAs("Src")]
        public string src;
        [Range(0, 100)]
        [FormerlySerializedAs("Volume")]
        public int volume = 100;
        [FormerlySerializedAs("Loop")]
        public bool loop = true;
        /// <summary>
        /// Auto play the video when start.
        /// </summary>
        public bool autoplay = true;
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
