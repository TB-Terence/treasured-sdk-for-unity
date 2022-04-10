using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Treasured.UnitySdk.Validation;
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

        public virtual List<ValidationResult> CanExport()
        {
            return new List<ValidationResult>();
        }

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
            var exporters = new Exporter[] { map.jsonExporter, map.cubemapExporter, map.meshExporter };
            List<ValidationResult> validationResults = new List<ValidationResult>();
            foreach (var exporter in exporters)
            {
                var results = exporter.CanExport();
                if(results != null)
                {
                    validationResults.AddRange(results);
                }
            }
            if(validationResults.Count > 0)
            {
                var errors = validationResults.Select(result => result.type == ValidationResult.ValidationResultType.Error);
                if(errors.Count() > 0)
                {
                    throw new ValidationException(validationResults);
                }
                foreach (var result in validationResults)
                {
                    string message = $"{result.name}\n{result.description}";
                    if(result.type == ValidationResult.ValidationResultType.Warning)
                    {
                        Debug.LogWarning(message, result.context);
                    }
                    else if(result.type == ValidationResult.ValidationResultType.Error)
                    {
                        Debug.LogError(message, result.context);
                    }
                }
                throw new Exception("Failed to export. Check console for more details.");
            }
            foreach (var exporter in exporters)
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
