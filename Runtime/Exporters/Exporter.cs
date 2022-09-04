﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Treasured.UnitySdk.Validation;
using UnityEngine;
using Newtonsoft.Json;

namespace Treasured.UnitySdk
{
    public abstract class Exporter : ScriptableObject, IExportHandler
    {
        public const string IconsDirectory = "icons";
        /// <summary>
        /// Reference to the map object for the export process. Used internally.
        /// </summary>
        [HideInInspector]
        [SerializeField]
        private TreasuredMap _map;
        [HideInInspector]
        public bool enabled = true;

        [JsonIgnore]
        public TreasuredMap Map { get => _map; }

        public virtual List<ValidationResult> CanExport()
        {
            return new List<ValidationResult>();
        }

        internal virtual void OnEnabled() { }
        internal virtual void OnDisabled() { }

        public virtual void OnPreExport() { }
        public virtual void Export() { }
        public virtual void OnPostExport() { }

        public static event EventHandler ExportCompleted = delegate { };

        public virtual DirectoryInfo CreateExportDirectoryInfo()
        {
            return Directory.CreateDirectory(Path.Combine(Map.projectFolder, ".treasured", Map.projectFolder));
        }

        public static void Export(TreasuredMap map)
        {
            var exporters = ReflectionUtils.GetSerializedFieldValuesOfType<Exporter>(map);
            List<ValidationResult> validationResults = new List<ValidationResult>();
            foreach (var exporter in exporters)
            {
                var results = exporter.CanExport();
                if(results != null)
                {
                    validationResults.AddRange(results);
                }
            }
            if(!map.exportOnSave && ((!TreasuredSDKPreferences.Instance.ignoreWarnings && validationResults.Count > 0) || (TreasuredSDKPreferences.Instance.ignoreWarnings && validationResults.Any(result => result.type == ValidationResult.ValidationResultType.Error))))
            {
                throw new ValidationException(validationResults);
            }
            ForceExport(map);
        }

        public static void ForceExport(TreasuredMap map)
        {
            if (string.IsNullOrWhiteSpace(map.projectFolder))
            {
                throw new ArgumentException($"Export Settings > Folder Name is empty.");
            }
            var exporters = ReflectionUtils.GetSerializedFieldValuesOfType<Exporter>(map);
            DataValidator.ValidateMap(map);
            var exportPath = Path.Combine(map.projectFolder, ".treasured", map.projectFolder);
            if (!Directory.Exists(exportPath))
            {
                Directory.CreateDirectory(exportPath); // try create the directory if not exist.
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

            ExportCompleted?.Invoke(null, EventArgs.Empty);
        }
    }
}
