using Newtonsoft.Json;
using System.IO;
using UnityEngine;

namespace Treasured.UnitySdk
{
    public class JsonExporter : Exporter
    {
        public static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ContractResolver = ContractResolver.Instance,
            CheckAdditionalContent = true
        };

        public Formatting formatting = Formatting.Indented;
        [Tooltip("Convert to Three JS Transform")]
        public bool convertTransform = true;

        public override void Export()
        {
            string jsonPath = Path.Combine(Settings.OutputDirectory, "data.json").Replace('/', '\\');
            string json = JsonConvert.SerializeObject(Map, formatting, JsonSettings);
            File.WriteAllText(jsonPath, json);
        }
    }
}
