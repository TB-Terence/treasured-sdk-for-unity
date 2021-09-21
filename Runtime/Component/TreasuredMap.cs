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

        #region Export Settings
        [SerializeField]
        [AssetSelector(true)]
        [Obsolete]
        private string _outputDirectory = "";
        #endregion

        [Obsolete]
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
        #endregion

        #region Launch Page
        public string Id { get => _id; }
        [TextArea(3, 3)]
        public string title;
        [TextArea(5, 5)]
        public string description;

        [JsonIgnore]
        public Texture2D cover;
        #endregion

        #region Export Properties
        public ImageFormat format = ImageFormat.PNG;
        public ImageQuality quality = ImageQuality.Low;
        #endregion

        #region Guide Tour
        public bool loop;
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
