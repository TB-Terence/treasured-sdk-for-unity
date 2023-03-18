using System;
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
        public TreasuredMap Map { get => _map; set => _map = value; }

        public virtual List<ValidationResult> CanExport()
        {
            return new List<ValidationResult>();
        }

        internal virtual void OnEnabled() { }
        internal virtual void OnDisabled() { }

        public virtual void OnPreExport() { }
        public virtual void Export() { }
        public virtual void OnPostExport() { }

        public virtual DirectoryInfo CreateExportDirectoryInfo()
        {
            return Directory.CreateDirectory(Path.Combine(Map.exportSettings.OutputDirectory));
        }

        public static void Export(TreasuredMap map)
        {
            var fieldValues = ReflectionUtilities.GetSerializableFieldValuesOfType<Exporter>(map);
            List<ValidationResult> validationResults = new List<ValidationResult>();
            foreach (var field in fieldValues)
            {
                var exporter = field.Value;
                var results = exporter.CanExport();
                if (results != null)
                    validationResults.AddRange(results);
            }
            if ((!TreasuredSDKPreferences.Instance.ignoreWarnings && validationResults.Count > 0) || (TreasuredSDKPreferences.Instance.ignoreWarnings && validationResults.Any(result => result.type == ValidationResult.ValidationResultType.Error)))
            {
                throw new ValidationException(validationResults);
            }
            ForceExport(map);
        }

        public static void ForceExport(TreasuredMap map)
        {
            if (string.IsNullOrWhiteSpace(map.exportSettings.folderName))
            {
                throw new ArgumentException($"Export Settings > Folder Name is empty.");
            }
            var exporterPairs = ReflectionUtilities.GetSerializableFieldValuesOfType<Exporter>(map);
            DataValidator.ValidateMap(map);
            var exportPath = Path.Combine(map.exportSettings.OutputDirectory);
            if (!Directory.Exists(exportPath))
            {
                Directory.CreateDirectory(exportPath); // try create the directory if not exist.
            }
            foreach (var pair in exporterPairs)
            {
                var exporter = pair.Value;
                if (!pair.Value.IsNullOrNone() && exporter.enabled)
                {
                    try
                    {
                        exporter.OnPreExport();
                        exporter.Export();
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        exporter.OnPostExport();
                    }
                }
            }

            if (map.cubemapExporter.enabled || map.meshExporter.enabled)
            {
                string argument;

                if (map.exportSettings.optimizeScene
                    || map.exportSettings.ExportType == ExportType.ProductionExport)
                {
                    argument =
                        $"treasured optimize \"{map.exportSettings.OutputDirectory}\"";
                }
                else
                {
                    argument =
                        $"treasured optimize \"{map.exportSettings.OutputDirectory}\" --skipGlb";
                    UnityEngine.Debug.LogError("optimizing skip glb");
                }

                // Run `treasured optimize` to optimize the glb file
                var npmProcess = ProcessUtilities.CreateProcess(argument);
                npmProcess.Start();
                string stdOutput = "";
                try
                {
                    npmProcess.Start();

#if UNITY_EDITOR
                    while (!npmProcess.HasExited)
                    {
                        if (UnityEditor.EditorUtility.DisplayCancelableProgressBar("Finalizing Export",
                            $"Please wait. Processing {map.exportSettings.folderName}...", 50 / 100f))
                        {
                            ProcessUtilities.KillProcess(npmProcess);
                            throw new OperationCanceledException();
                        }
                    }
#endif

                    stdOutput = npmProcess.StandardOutput.ReadToEnd();
                }
                catch (OperationCanceledException e)
                {
#if UNITY_EDITOR
                    UnityEditor.EditorUtility.DisplayDialog("Canceled", e.Message, "OK");
#endif
                }
                catch (Exception e)
                {
                    throw new ApplicationException(e.Message);
                }
                finally
                {
                    if (!string.IsNullOrEmpty(stdOutput))
                    {
                        UnityEngine.Debug.Log(stdOutput);
                    }

                    npmProcess?.Dispose();
#if UNITY_EDITOR
                    UnityEditor.EditorUtility.ClearProgressBar();
#endif
                }
            }
        }
    }
}
