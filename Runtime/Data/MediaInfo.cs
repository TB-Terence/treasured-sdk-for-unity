using Newtonsoft.Json;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.Video;

namespace Treasured.UnitySdk
{
    [Serializable]
    public abstract class MediaInfo<T> where T : UnityEngine.Object
    {
        [JsonIgnore]
        [OnValueChanged(nameof(UpdateUri))]
        public T asset;
        [SerializeField]
        [ShowIf(nameof(IsLocalContent))]
        [ReadOnly]
        [TextArea(3, 5)]
        private string _localUri = String.Empty;
        [SerializeField]
        [ShowIf(nameof(IsRemoteContent))]
        [TextArea(3, 5)]
        private string _remoteUri = String.Empty;

        [JsonProperty("uri")]
        public string Uri
        {
            get
            {
                return IsLocalContent() ? _localUri : _remoteUri;
            }
            set
            {
                if (IsLocalContent())
                {
                    _localUri = value;
                }
                else
                {
                    _remoteUri = value;
                }
            }
        }

        bool IsRemoteContent()
        {
            return !IsLocalContent();
        }

        public bool IsLocalContent()
        {
            return !asset.IsNullOrNone();
        }

        public abstract string GetLocalPathPrefix();

        void UpdateUri()
        {
#if UNITY_EDITOR
            if (IsLocalContent())
            {
                _localUri = $"{GetLocalPathPrefix()}/" + Path.GetFileName(UnityEditor.AssetDatabase.GetAssetPath(asset));
            }
#endif
        }
    }

    [Serializable]
    public sealed class AudioInfo : MediaInfo<AudioClip>
    {
        [Range(0, 100)]
        public int volume = 50;
        public bool muted;
        public bool loop;
        public bool autoplay;

        public override string GetLocalPathPrefix()
        {
            return "audio";
        }
    }

    [Serializable]
    public sealed class ImageInfo : MediaInfo<Texture2D>
    {
        public override string GetLocalPathPrefix()
        {
            return "images";
        }
    }

    [Serializable]
    public sealed class VideoInfo : MediaInfo<VideoClip>
    {
        [Range(0, 100)]
        public int volume = 50;
        public bool muted;
        public bool loop;
        public bool autoplay;

        public override string GetLocalPathPrefix()
        {
            return "video";
        }
    }
}
