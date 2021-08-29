using System;
using System.Collections.Generic;
using Treasured.SDK;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [Serializable]
    public sealed class TreasuredMapData
    {
        public static readonly string Version = "0.4.0";

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

        public string Title { get => _title; set => _title = value; }
        public string Description { get => _description; set => _description = value; }
        public bool Loop { get => _loop; set => _loop = value; }
        public ImageFormat Format { get => _format; set => _format = value; }
        public ImageQuality Quality { get => _quality; set => _quality = value; }

        public List<HotspotData> Hotspots { get; private set; } = new List<HotspotData>();
        public List<InteractableData> Interactables { get; private set; } = new List<InteractableData>();

        private TreasuredMapData() { }

        public void GenerateHotspots(IEnumerable<Hotspot> hotspots)
        {
            Hotspots.Clear();
            foreach (var hotspot in hotspots)
            {
                if (!hotspot.gameObject.activeSelf)
                {
                    continue;
                }
                Hotspots.Add(hotspot);
            }
        }

        public void GenerateInteractables(IEnumerable<Interactable> interactables)
        {
            Interactables.Clear();
            foreach (var interactable in interactables)
            {
                if (!interactable.gameObject.activeSelf)
                {
                    continue;
                }
                Interactables.Add(interactable);
            }
        }
    }
}
