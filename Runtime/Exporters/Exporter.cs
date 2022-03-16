using System;
using System.IO;
using UnityEngine;

namespace Treasured.UnitySdk
{
    public abstract class Exporter : ScriptableObject, IExportHandler
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

        public virtual void OnPreExport() { }
        public abstract void Export();
        public virtual void OnPostExport() { }

        public static void Export(TreasuredMap map)
        {
            if (string.IsNullOrEmpty(map.exportSettings.folderName))
            {
                throw new MissingFieldException("Output folder name is empty.");
            }
            DataValidator.ValidateMap(map);
            if (Directory.Exists(map.exportSettings.OutputDirectory))
            {
                Directory.Delete(map.exportSettings.OutputDirectory, true);
            }
            Directory.CreateDirectory(map.exportSettings.OutputDirectory); // try create the directory if not exist.
            foreach (var exporter in new Exporter[] { map.jsonExporter, map.cubemapExporter, map.meshExporter })
            {
                if (exporter != null && exporter.enabled)
                {
                    exporter.OnPreExport();
                    exporter.Export();
                    exporter.OnPostExport();
                }
            }
        }
    }
}
