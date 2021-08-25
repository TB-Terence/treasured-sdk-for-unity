using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Treasured.SDK
{
    public enum ImageFormat
    {
        JPG,
        PNG
    }
    public enum ImageQuality
    {
        Low = 1024,
        Medium = 2048,
        High = 4096,
        Ultra = 8192
    }

    [CreateAssetMenu(fileName = "Data", menuName = "Treasured/Data")]
    public sealed class TreasuredData : ScriptableObject
    {
        public static readonly string Version = "0.3.6";

        [SerializeField]
        private string _name;
        [SerializeField]
        private bool _loop;
        [SerializeField]
        private ImageFormat _format = ImageFormat.PNG;
        [SerializeField]
        private ImageQuality _quality = ImageQuality.Medium;
        [SerializeField]
        private List<TreasuredObject> _hotspots = new List<TreasuredObject>();
        [SerializeField]
        private List<TreasuredObject> _interactables = new List<TreasuredObject>();

        public string Name { get => _name; set => _name = value; }
        public bool Loop { get => _loop; set => _loop = value; }
        public ImageFormat Format { get => _format; set => _format = value; }
        public ImageQuality Quality { get => _quality; set => _quality = value; }
        public List<TreasuredObject> Hotspots { get => _hotspots; set => _hotspots = value; }
        public List<TreasuredObject> Interactables { get => _interactables; set => _interactables = value; }
        /// <summary>
        /// Returns a unique list of objects.
        /// </summary>
        [JsonIgnore]
        public IEnumerable<TreasuredObject> All
        {
            get
            {
                return Hotspots.Union(Interactables);
            }
        }
    }
}
