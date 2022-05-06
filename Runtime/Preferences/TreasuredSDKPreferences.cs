using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Treasured.UnitySdk
{
    public sealed class TreasuredSDKPreferences : ScriptableObject
    {
        public static string PreferenceFilePath
        {
            get
            {
                return InternalEditorUtility.unityPreferencesFolder + "/TreasuredSDKPreferences.json";
            }
        }

        private static TreasuredSDKPreferences s_instance;
        public static TreasuredSDKPreferences Instance
        {
            get
            {
                if ((UnityEngine.Object)s_instance == (UnityEngine.Object)null)
                {
                    Create();
                    Load(PreferenceFilePath);
                }
                return s_instance;
            }
        }

        [Header("Gizmos")]
        //[Tooltip("Auto focus on Treasured Object when being selected.")]
        //public bool autoFocus = true;
        [Tooltip("Gizmos color for hotspot camera")]
        public Color32 frustumColor = Color.red;

        [Header("Export Settings")]
        [Tooltip("Ignore warnings when export(E.g., Hotspot path blocked by collider).")]
        public bool ignoreWarnings;
        public string customExportFolder;
        [JsonProperty(ItemTypeNameHandling = TypeNameHandling.Auto)]
        public List<Exporter> exporters = new List<Exporter>();

        TreasuredSDKPreferences()
        {
            if ((UnityEngine.Object)s_instance != (UnityEngine.Object)null)
            {
                Debug.LogError("ScriptableSingleton already exists. Did you query the singleton in a constructor?");
            }
            else
            {
                s_instance = this;
            }
        }

        static void Create()
        {
            if ((UnityEngine.Object)s_instance == (UnityEngine.Object)null)
            {
                s_instance = ScriptableObject.CreateInstance<TreasuredSDKPreferences>();
                s_instance.customExportFolder = Path.Combine(Application.dataPath, "Treasured Data").Replace("\\", "/");
            }
        }

        static void Load(string path)
        {
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                string json = File.ReadAllText(path);
                JsonConvert.PopulateObject(json, s_instance, JsonExporter.JsonSettings);
                s_instance.InitializeMissingExporters();
            }
        }

        void InitializeMissingExporters()
        {
            exporters ??= new List<Exporter>();
            var exporterTypes = TypeCache.GetTypesDerivedFrom<Exporter>().ToList();
            // remove existing types
            foreach (var exporter in exporters)
            {
                exporterTypes.Remove(exporter.GetType());
            }
            foreach (var missingType in exporterTypes)
            {
                exporters.Add((Exporter)ScriptableObject.CreateInstance(missingType));
            }
        }

        public void Save()
        {
            InitializeMissingExporters();
            string value = JsonConvert.SerializeObject(Instance, Formatting.Indented, JsonExporter.JsonSettings);
            File.WriteAllText(PreferenceFilePath, value);
        }

        public SerializedObject GetSerializedObject()
        {
            return new SerializedObject(this);
        }

        public static void UseDefaults()
        {
            GameObject.DestroyImmediate(s_instance);
            Create();
        }
    }
}