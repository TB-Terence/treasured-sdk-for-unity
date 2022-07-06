using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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

        #region Export Settings
        [SerializeField]
        private string _outputFolderName;
        #endregion

        [Code]
        public string headHTML;

        //public Dictionary<string, string> SvgData
        //{
        //    get
        //    {
        //        Dictionary<string, string> svgData = new Dictionary<string, string>();
        //        foreach (var obj in GetComponentsInChildren<TreasuredObject>())
        //        {
        //            if (obj.button == null || obj.button.icon2 == null || svgData.ContainsKey(obj.button.icon2.name) || string.IsNullOrWhiteSpace(obj.button.icon2.svg))
        //            {
        //                continue;
        //            }
        //            // TODO : Validate XML file
        //            svgData.Add(obj.button.icon2.name, obj.button.icon2.svg);
        //        }
        //        return svgData;
        //    }
        //}

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

        private void OnValidate()
        {
            //  Set default Auto Camera Rotation to false for all except for modern loader template 
            if (_templateLoader != null && _templateLoader.template != "modern")
            {
                _templateLoader.autoCameraRotation = false;
            }
        }

        private void Reset()
        {
            var fields = ReflectionUtils.GetSeriliazedFieldReferences(this, false).Where(x => typeof(ScriptableObject).IsAssignableFrom(x.fieldInfo.FieldType));
            foreach (var field in fields)
            {
                field.SetValue(ScriptableObject.CreateInstance(field.fieldInfo.FieldType));
            }
        }
    }
}
