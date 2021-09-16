using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Treasured.UnitySdk
{
    public enum Format
    {
        PNG,
        JPG
    }

    public enum Quality
    {
        Low,
        Medium,
        High
    }

    [CreateAssetMenu(menuName = "Treasured/Treasured Map", fileName = "New Treasured Map")]
    public sealed class TreasuredMap : ScriptableObject
    {
        [JsonProperty]
        public static readonly string Version = typeof(TreasuredMap).Assembly.GetName().Version.ToString();

        [SerializeField]
        [ReadOnly]
        private string _id = Guid.NewGuid().ToString();
        public string Id { get => _id; internal set => _id = value; }

        [TextArea(2, 2)]
        public string title;

        [TextArea(5, 5)]
        public string description;

        public Format format = Format.PNG;

        public Quality quality = Quality.Medium;

        public bool loop;

        [SerializeField]
        private List<Hotspot> hotspots = new List<Hotspot>();

        [SerializeField]
        private List<Interactable> interactables = new List<Interactable>();

        public List<Hotspot> Hotspots
        {
            get => hotspots;
        }

        public List<Interactable> Interactables
        {
            get => interactables;
        }

        private void OnEnable()
        {
            if (string.IsNullOrEmpty(_id))
            {
                _id = Guid.NewGuid().ToString();
            }
        }

        void Reset()
        {
#if UNITY_EDITOR
            if (UnityEditor.EditorUtility.IsPersistent(this))
            {
                string assetPath = UnityEditor.AssetDatabase.GetAssetPath(this);
                foreach (var subAsset in UnityEditor.AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath))
                {
                    UnityEditor.AssetDatabase.RemoveObjectFromAsset(subAsset);
                }
                UnityEditor.AssetDatabase.SaveAssets();
            }
#endif
        }

        public IEnumerable<ObjectBase> GetObjects()
        {
            foreach (var hotspot in hotspots)
            {
                yield return hotspot;
            }
            foreach (var interactable in interactables)
            {
                yield return interactable;
            }
        }

        public bool TryGetObject(string id, out ObjectBase obj)
        {
            obj = GetObjects().FirstOrDefault(x => x.Id.Equals(id));
            return obj != null;
        }
    }
}
