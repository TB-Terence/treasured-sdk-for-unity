using Newtonsoft.Json;
using System;
using System.IO;
using UnityEngine;

namespace Treasured.UnitySdk
{
    public class TreasuredSceneManager : MonoBehaviour
    {
        [Group]
        public SceneInfo sceneInfo;
        [Group]
        public StyleInfo styleInfo;

        [JsonIgnore]
        [Group("Export Settings")]
        public ExportSettings exportSettings;

        [JsonIgnore]
        [Group("Export Settings")]
        public JsonExporter jsonExporter;

        [JsonIgnore]
        [Group("Export Settings")]
        public IconExporter iconExporter;

        [JsonIgnore]
        [Group("Export Settings")]
        public CubemapExporter cubemapExporter;

        [JsonIgnore]
        [Group("Export Settings")]
        public MeshExporter meshExporter;

        [JsonIgnore]
        [Group("Export Settings")]
        public AudioExporter audioExporter;

        [JsonIgnore]
        [Group("Export Settings")]
        public VideoExporter VideoExporter;
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
        [OnValueChanged(nameof(UpdateUri))]
        public T asset;
        [EnableIf(nameof(IsRemoteContent))]
        [TextArea(3, 5)]
        [SerializeField]
        private string _uri;
        public string Uri { get => _uri; }

        bool IsRemoteContent()
        {
            return asset.IsNullOrNone();
        }

        void UpdateUri()
        {
#if UNITY_EDITOR
            _uri = !asset.IsNullOrNone() ? "audio/" + Path.GetFileName(UnityEditor.AssetDatabase.GetAssetPath(asset)) : string.Empty;
#endif
        }
    }

    [System.Serializable]
    public sealed class AudioContent : Content<AudioClip>
    {
        [Range(0, 100)]
        public int volume;
        public bool muted;
    }
}
