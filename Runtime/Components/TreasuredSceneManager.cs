using Newtonsoft.Json;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.Video;

namespace Treasured.UnitySdk
{
    public class TreasuredSceneManager : MonoBehaviour
    {
        public SceneInfo sceneInfo;
        public StyleInfo styleInfo;

        private void OnValidate()
        {
            
        }
    }

    [System.Serializable]
    public class SceneInfo
    {
        [SerializeField]
        [ReadOnly]
        private string _id = Guid.NewGuid().ToString();
        public string Id { get => _id; }

        [RequiredField]
        [Tooltip("The name of the individual, company or organization.")]
        public string creator;
        [RequiredField]
        [TextArea(2, 2)]
        public string title;
        [RequiredField]
        [TextArea(3, 5)]
        public string description;

        public AudioContent backgroundMusic;
    }

    [System.Serializable]
    public class StyleInfo
    {
        [JsonProperty("loader")]
        public TemplateLoader templateLoader;
        public bool darkMode = false;
    }

    [System.Serializable]
    public abstract class Content<T> where T : UnityEngine.Object
    {
        [JsonIgnore]
        [OnValueChanged(nameof(UpdateUri))]
        public T asset;
        [SerializeField]
        [ShowIf(nameof(IsLocalContent))]
        [ReadOnly]
        [TextArea(3, 5)]
        [JsonIgnore]
        private string _localUri;
        [ShowIf(nameof(IsRemoteContent))]
        [TextArea(3, 5)]
        [JsonIgnore]
        public string remoteUri;

        [JsonProperty("uri")]
        public string Uri
        {
            get
            {
                return IsLocalContent() ? _localUri : remoteUri;
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
    public sealed class AudioContent : Content<AudioClip>
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
    public sealed class ImageContent : Content<Texture2D>
    {
        public override string GetLocalPathPrefix()
        {
            return "images";
        }
    }

    [Serializable]
    public sealed class VideoContent : Content<VideoClip>
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
