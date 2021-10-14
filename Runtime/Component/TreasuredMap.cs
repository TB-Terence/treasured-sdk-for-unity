using Newtonsoft.Json;
using System;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Treasured/Treasured Map")]
    public sealed class TreasuredMap : MonoBehaviour
    {
        #region Map Data
        [SerializeField]
        [Obsolete]
        private TreasuredMapData _data = new TreasuredMapData();
        #endregion

        [JsonIgnore]
        public TreasuredMapData Data
        {
            get
            {
                if (_data == null)
                {
                    _data = new TreasuredMapData();
                }
                return _data;
            }
            set
            {
                _data = value;
            }
        }

        #region JSON properties
        [JsonProperty]
        public static readonly string Version = typeof(TreasuredMap).Assembly.GetName().Version.ToString();
        [JsonProperty]
        private DateTime created
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
        [TextArea(3, 3)]
        private string _title;
        public string Title { get => _title; set => _title = value; }

        [SerializeField]
        [TextArea(5, 5)]
        private string _description;
        public string Description { get => _description; set => _title = _description; }

        [JsonIgnore]
        [SerializeField]
        private Texture2D _cover;
        #endregion

        #region Guide Tour
        [SerializeField]
        private bool _loop;

        public bool Loop { get => _loop; set => _loop = value; }
        #endregion

        #region Export Properties
        [SerializeField]
        private ImageFormat _format = ImageFormat.PNG;
        public ImageFormat Format { get => _format; set => _format = value; }

        [SerializeField]
        private ImageQuality _quality = ImageQuality.High;
        public ImageQuality Quality { get => _quality; set => _quality = value; }

        public Color32 MaskColor => Color.white;
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
        #endregion

        #region Export Settings
        [SerializeField]
        private string _outputFolderName;
        [JsonIgnore]
        public string OutputFolderName { get => _outputFolderName; set => _outputFolderName = value; }

        [SerializeField]
        private int _interactableLayer; // game object can only have one layer thus using int instead of LayerMask
        #endregion
    }
}
