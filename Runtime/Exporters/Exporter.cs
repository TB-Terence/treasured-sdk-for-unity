using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Treasured.UnitySdk.Validation;
using UnityEngine;
using UnityEngine.Serialization;
using Newtonsoft.Json;

namespace Treasured.UnitySdk
{
    public abstract class Exporter : ScriptableObject, IExportHandler
    {
        public const string IconsDirectory = "icons";
        /// <summary>
        /// Reference to the map object for the export process. Used internally.
        /// </summary>
        [FormerlySerializedAs("_map")]
        [HideInInspector]
        [SerializeField]
        private TreasuredScene _scene;
        [HideInInspector]
        public bool enabled = true;

        [JsonIgnore]
        public TreasuredScene Scene { get => _scene; set => _scene = value; }

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
            return Directory.CreateDirectory(Path.Combine(Scene.exportSettings.OutputDirectory));
        }

        public static void Export(TreasuredScene scene)
        {
            var fieldValues = ReflectionUtilities.GetSerializableFieldValuesOfType<Exporter>(scene);
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
            ForceExport(scene);
        }

        public static void ForceExport(TreasuredScene scene)
        {
            if (string.IsNullOrWhiteSpace(scene.exportSettings.folderName))
            {
                throw new ArgumentException($"Export Settings > Folder Name is empty.");
            }
            var exporterPairs = ReflectionUtilities.GetSerializableFieldValuesOfType<Exporter>(scene);
            DataValidator.ValidateMap(scene);
            var exportPath = scene.exportSettings.OutputDirectory;
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

            if (scene.cubemapExporter.enabled || scene.meshExporter.enabled)
            {
                string argument;

                if (scene.exportSettings.optimizeScene
                    || scene.exportSettings.ExportType == ExportType.ProductionExport)
                {
                    argument =
                        $"treasured optimize \"{scene.exportSettings.OutputDirectory}\"";
                }
                else
                {
                    argument =
                        $"treasured optimize \"{scene.exportSettings.OutputDirectory}\" --skipGlb";
                    //UnityEngine.Debug.LogError("optimizing skip glb");
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
                            $"Please wait. Processing {scene.exportSettings.folderName}...", 50 / 100f))
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
