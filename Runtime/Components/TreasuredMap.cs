using Newtonsoft.Json;
using System;
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

        [SerializeField]
        [TextArea(3, 3)]
        [JsonProperty("audioUrl")]
        private string _audioUrl;
        [SerializeField]
        [JsonProperty("muteOnStart")]
        private bool _muteOnStart;
        [SerializeField]
        [JsonProperty("loader")]
        private TemplateLoader _templateLoader;
        #endregion

        #region Guide Tour
        [SerializeField]
        private bool _loop;

        public bool Loop { get => _loop; set => _loop = value; }
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

        public SoundSource[] Sounds
        {
            get
            {
                return GetComponentsInChildren<SoundSource>();
            }
        }
        #endregion

        #region Export Settings
        [SerializeField]
        private string _outputFolderName;
        #endregion

        [Code]
        public string headHTML;

        [JsonIgnore]
        public ExportSettings exportSettings;

        [JsonIgnore]
        public JsonExporter jsonExporter;

        [JsonIgnore]
        public CubemapExporter cubemapExporter;

        [JsonIgnore]
        public MeshExporter meshExporter;

        private void OnValidate()
        {
            //  Set default Auto Camera Rotation to false for all except for modern loader template 
            if (_templateLoader != null && _templateLoader.template != "modern")
            {
                _templateLoader.autoCameraRotation = false;
            }
        }
    }
}
