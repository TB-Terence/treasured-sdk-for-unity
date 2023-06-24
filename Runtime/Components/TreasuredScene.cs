using Newtonsoft.Json;
using System;
using System.Diagnostics;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [DisallowMultipleComponent]
    public class TreasuredScene : MonoBehaviour
    {
        [JsonProperty]
        public static readonly string Version = typeof(TreasuredMap).Assembly.GetName().Version.ToString();

        [JsonProperty]
        public DateTime Created
        {
            get
            {
                return DateTime.Now;
            }
        }

        [Serializable]
        public class SceneInfo
        {
            public bool loopHotspots = true;
            public AudioInfo backgroundMusicInfo;

            public SceneInfo()
            {
                backgroundMusicInfo = new AudioInfo();
            }
        }


        [Serializable]
        public class ThemeInfo
        {
            [JsonProperty("loader")]
            public TemplateLoader templateLoader = new TemplateLoader();
            public bool darkMode = false;
        }

        [SerializeField]
        [ReadOnly]
        [GUID]
        internal string _id = Guid.NewGuid().ToString();
        public string Id { get => _id; }

        public Thumbnail thumbnail = new Thumbnail();

        [JsonProperty("author")]
        [Tooltip("The name of the individual, company or organization.")]
        public string creator;
        [RequiredField]
        [TextArea(2, 2)]
        public string title;
        [RequiredField]
        [TextArea(3, 5)]
        public string description;

        public SceneInfo sceneInfo = new SceneInfo();
        public ThemeInfo themeInfo = new ThemeInfo();

        [JsonIgnore]
        public ExportSettings exportSettings;

        [JsonIgnore]
        public JsonExporter jsonExporter;

        [JsonIgnore]
        public IconExporter iconExporter;

        [JsonIgnore]
        public CubemapExporter cubemapExporter;

        [JsonIgnore]
        public MeshExporter meshExporter;

        [JsonIgnore]
        public AudioExporter audioExporter;

        [JsonIgnore]
        public VideoExporter VideoExporter;

        #region Browser Settings
        public UISettings uiSettings = new UISettings();
        #endregion

        [JsonProperty("guidedTours")]
        public GuidedTourGraph graph;

        #region Features
        public FeatureSettings features = new FeatureSettings();
        #endregion

        #region Objects
        public Hotspot[] Hotspots
        {
            get
            {
                return GetComponentsInChildren<Hotspot>();
            }
        }
        public Interactable[] Interactables
        {
            get
            {
                return GetComponentsInChildren<Interactable>();
            }
        }

        public VideoRenderer[] Videos
        {
            get
            {
                return GetComponentsInChildren<VideoRenderer>();
            }
        }

        public SoundSource[] Audios
        {
            get
            {
                return GetComponentsInChildren<SoundSource>();
            }
        }

        public HTMLEmbed[] HTMLEmbeds
        {
            get
            {
                return GetComponentsInChildren<HTMLEmbed>();
            }
        }
        #endregion

        #region Deprecated Proprties
        [JsonProperty]
        TemplateLoader loader => themeInfo.templateLoader;
        [JsonProperty]
        bool loop => sceneInfo.loopHotspots;
        [JsonProperty]
        string audioUrl => sceneInfo.backgroundMusicInfo.Path; 
        [JsonProperty]
        bool muteOnStart => sceneInfo.backgroundMusicInfo.muted;
        [JsonProperty]
        string format => "ktx2";
        #endregion

        private void OnValidate()
        {
            //  Set default Auto Camera Rotation to false for all except for modern loader template 
            if (themeInfo.templateLoader != null && themeInfo.templateLoader.template != "modern")
            {
                themeInfo.templateLoader.autoCameraRotation = false;
            }

            uiSettings.darkMode = themeInfo.darkMode;
        }
    }
}
