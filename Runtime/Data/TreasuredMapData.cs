using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [Serializable]
    public sealed class TreasuredMapData
    {
        public static readonly string Version = "0.4.1";

        /// <summary>
        /// The unique identifier of the map.
        /// </summary>
        [SerializeField]
        [GUID]
        private string _id = Guid.NewGuid().ToString();

        /// <summary>
        /// The title of the exhibit on the landing page.
        /// </summary>
        [SerializeField]
        [TextArea(2, 2)]
        [Tooltip("The title of the exhibit on the landing page.")]
        private string _title;

        /// <summary>
        /// The description of the exhibit on the landing page.
        /// </summary>
        [SerializeField]
        [TextArea(3, 5)]
        [Tooltip("The description of the exhibit on the landing page.")]
        private string _description;

        [SerializeField]
        [Tooltip("The last Hotspot should go back to the first Hotspot in the Guide Tour mode if enabled.")]
        private bool _loop;

        [SerializeField]
        private ImageFormat _format = ImageFormat.PNG;

        [SerializeField]
        private ImageQuality _quality = ImageQuality.High;

        public string Id { get => _id; }
        public string Title { get => _title; set => _title = value; }
        public string Description { get => _description; set => _description = value; }
        public bool Loop { get => _loop; set => _loop = value; }
        public ImageFormat Format { get => _format; set => _format = value; }
        public ImageQuality Quality { get => _quality; set => _quality = value; }

        [SerializeField]
        private List<HotspotData> _hotspots = new List<HotspotData>();
        [SerializeField]
        private List<InteractableData> _interactables = new List<InteractableData>();

        public List<HotspotData> Hotspots { get => _hotspots; }
        public List<InteractableData> Interactables { get => _interactables; }

        [JsonConstructor]
        private TreasuredMapData(string id)
        {
            this._id = id;
        }

        internal TreasuredMapData() { }


    }
}
