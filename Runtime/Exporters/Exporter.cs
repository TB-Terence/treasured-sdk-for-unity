using UnityEngine;

namespace Treasured.UnitySdk
{
    public abstract class Exporter : ScriptableObject
    {
        /// <summary>
        /// Reference to the map object for the export process. Used internally.
        /// </summary>
        [HideInInspector]
        [SerializeField]
        private TreasuredMap _map;
        [HideInInspector]
        public bool enabled = true;

        public TreasuredMap Map { get => _map; }
        public ExportSettings Settings { get => _map?.exportSettings; }

        public virtual void OnPreExport() { }
        public abstract void Export();
        public virtual void OnPostExport() { }
    }
}
