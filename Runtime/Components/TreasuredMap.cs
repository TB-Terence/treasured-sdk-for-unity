using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Serialization;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Treasured/Treasured Map")]
    public sealed class TreasuredMap : MonoBehaviour
    {
        #region JSON properties
        [JsonProperty]
        public static readonly string Version = typeof(TreasuredMap).Assembly.GetName().Version.ToString();
        [JsonProperty("created")]
        private DateTime Created
        {
            get
            {
                return DateTime.Now;
            }
        }
        #endregion

        #region Map properties
        
        [SerializeField]
        [GUID]
        private string _id = Guid.NewGuid().ToString();
        public string Id { get => _id; }
        #endregion

        #region Launch Page
        [RequiredField]
        [SerializeField]
        private string _author;
        public string Author { get => _author; set => _author = value; }
        [RequiredField]
        [SerializeField]
        [TextArea(3, 3)]
        private string _title;
        public string Title { get => _title; set => _title = value; }

        [RequiredField]
        [SerializeField]
        [TextArea(5, 5)]
        private string _description;
        public string Description { get => _description; set => _title = _description; }

        [TextArea(3, 3)]
        [FormerlySerializedAs("_audioUrl")]
        public string audioUrl;
        [Range(0, 100)]
        public int defaultBackgroundVolume = 100;
        [FormerlySerializedAs("_muteOnStart")]
        public bool muteOnStart;
        [FormerlySerializedAs("_templateLoader")]
        [JsonProperty("loader")]
        public TemplateLoader templateLoader;
        #endregion

        #region Guide Tour
        [FormerlySerializedAs("_loop")]
        public bool loop;

        [JsonProperty("guidedTours")]
        public GuidedTourGraph graph;
        #endregion

        #region Browser Settings
        public UISettings uiSettings = new UISettings();
        #endregion

        #region Features
        public FeatureSettings features = new FeatureSettings();
        #endregion

        #region Export Properties
        public ImageFormat Format { get => cubemapExporter.imageFormat; }
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

        [Code]
        public string headHTML;

        public CustomEmbed[] pageEmbeds = new CustomEmbed[0];

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

        private void OnValidate()
        {
            //  Set default Auto Camera Rotation to false for all except for modern loader template 
            if (templateLoader != null && templateLoader.template != "modern")
            {
                templateLoader.autoCameraRotation = false;
            }
        }
    }
}
