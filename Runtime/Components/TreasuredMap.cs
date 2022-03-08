using Newtonsoft.Json;
using System;
using UnityEngine;
using Treasured.UnitySdk.Export;

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
        [SerializeField]
        private string _author;
        public string Author { get => _author; set => _author = value; }
        [SerializeField]
        [TextArea(3, 3)]
        private string _title;
        public string Title { get => _title; set => _title = value; }

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

        #region Export Properties
        [SerializeField]
        private ImageFormat _format = ImageFormat.Ktx2;
        public ImageFormat Format { get => _format; set => _format = value; }

        [SerializeField]
        private ImageQuality _quality = ImageQuality.High;
        public ImageQuality Quality { get => _quality; set => _quality = value; }

        [SerializeField]
        private bool _canUseTag;
        
        [JsonIgnore]
        public bool CanUseTag
        {
            get => _canUseTag;
            set => _canUseTag = value;
        }

        [SerializeField]
        private string _filterTag = "Untagged";
        
        [JsonIgnore]
        public string FilterTag
        {
            get => _filterTag;
            set => _filterTag = value;
        }

        [SerializeField]
        private bool _canUseLayerMask;
        
        [JsonIgnore]
        public bool CanUseLayerMask
        {
            get => _canUseLayerMask;
            set => _canUseLayerMask = value;
        }

        [SerializeField]
        private LayerMask _filterLayerMask;
        
        [JsonIgnore]
        public LayerMask FilterLayerMask
        {
            get => _filterLayerMask;
            set => _filterLayerMask = value;
        }

        [SerializeField]
        private bool _keepCombinedMesh;
        
        [JsonIgnore]
        public bool KeepCombinedMesh
        {
            get => _keepCombinedMesh;
            set => _keepCombinedMesh = value;
        }
        
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
        [JsonIgnore]
        public string OutputFolderName { get => _outputFolderName; set => _outputFolderName = value; }
        

        [SerializeField]
        private int _interactableLayer; // game object can only have one layer thus using int instead of LayerMask

        #endregion

        [Code]
        public string headHTML;

        [JsonIgnore]
        public ExportSettings exportSettings;

        [JsonIgnore]
        public JsonExportProcess jsonExportProcess;

        [JsonIgnore]
        public CubemapExportProcess cubemapExportProcess;

        [JsonIgnore]
        public MeshExportProcess meshExportProcess;

        private void OnValidate()
        {
            //  Set default Auto Camera Rotation to false for all except for modern loader template 
            if (_templateLoader.template != "modern")
            {
                _templateLoader.autoCameraRotation = false;
            }
        }
    }
}
