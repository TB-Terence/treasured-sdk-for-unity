using Newtonsoft.Json;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Serialization;

namespace Treasured.UnitySdk
{
    [Serializable]
    public abstract class MediaInfo<T> where T : UnityEngine.Object
    {
        [JsonIgnore]
        [OnValueChanged(nameof(UpdatePath))]
        public T asset;
        [SerializeField]
        [ShowIf(nameof(IsLocalContent))]
        [ReadOnly]
        [TextArea(3, 5)]
        [FormerlySerializedAs("_localUri")]
        private string _localPath = String.Empty;
        [SerializeField]
        [ShowIf(nameof(IsRemoteContent))]
        [TextArea(3, 5)]
        [FormerlySerializedAs("_remoteUri")]
        private string _remotePath = String.Empty;

        [JsonProperty("path")]
        public string Path
        {
            get
            {
                return IsLocalContent() ? _localPath : _remotePath;
            }
            set
            {
                if (IsLocalContent())
                {
                    _localPath = value;
                }
                else
                {
                    _remotePath = value;
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

        void UpdatePath()
        {
#if UNITY_EDITOR
            if (IsLocalContent())
            {
                _localPath = $"{GetLocalPathPrefix()}/" + System.IO.Path.GetFileName(UnityEditor.AssetDatabase.GetAssetPath(asset));
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
            return "audios";
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
