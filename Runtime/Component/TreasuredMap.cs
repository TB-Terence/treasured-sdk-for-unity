using Newtonsoft.Json;
using System;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Treasured/Treasured Map")]
    public sealed class TreasuredMap : MonoBehaviour
    {
        #region Map Settings
        [SerializeField]
        [Obsolete]
        private LayerMask _interactableLayer;
        #endregion

        #region Map Data
        [SerializeField]
        [Obsolete]
        private TreasuredMapData _data = new TreasuredMapData();
        #endregion

        [Obsolete]
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

        #region Map properties
        [JsonProperty]
        public static readonly string Version = typeof(TreasuredMap).Assembly.GetName().Version.ToString();
        [SerializeField]
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

        #region Export Properties
        [SerializeField]
        private ImageFormat _format = ImageFormat.PNG;
        public ImageFormat Format { get => _format; set => _format = value; }

        [SerializeField]
        private ImageQuality _quality = ImageQuality.High;
        public ImageQuality Quality { get => _quality; set => _quality = value; }
        #endregion

        #region Guide Tour
        [SerializeField]
        private bool _loop;

        public bool Loop { get => _loop; set => _loop = value; }
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
        public string OutputFolderName { get => _outputFolderName; set => _outputFolderName = value; }
        #endregion
    }
}
