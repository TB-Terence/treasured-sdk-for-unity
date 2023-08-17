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

        public VideoInfo videoInfo;

        [Url]
        [JsonIgnore]
        [FormerlySerializedAs("Src")]
        public string src;
        [Range(0, 100)]
        [FormerlySerializedAs("Volume")]
        [JsonIgnore]
        public int volume = 100;
        [FormerlySerializedAs("Loop")]
        [JsonIgnore]
        public bool loop = true;
        /// <summary>
        /// Auto play the video when start.
        /// </summary>
        [JsonIgnore]
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
        // TODO: Remove these after use new format
        #region Deprecated Properties
        [JsonProperty("src")]
        string Src
        {
            get
            {
                return videoInfo.Path;
            }
        }

        [JsonProperty("loop")]
        bool Loop
        {
            get
            {
                return videoInfo.loop;
            }
        }

        [JsonProperty("volume")]
        int Volume
        {
            get
            {
                return videoInfo.volume;
            }
        }

        [JsonProperty("autoPlay")]
        bool AutoPlay
        {
            get
            {
                return videoInfo.autoplay;
            }
        }
        #endregion
    }
}
