using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using Treasured.UnitySdk.Validation;

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

        public Newtonsoft.Json.Formatting formatting = Formatting.Indented;
        
        [UnityEngine.ContextMenu("Reset")]
        private void Reset()
        {
            enabled = true;
            formatting = Formatting.Indented;
        }

        public override void Export()
        {
            string jsonPath = Path.Combine(Map.exportSettings.OutputDirectory, "data.json");
            string json = JsonConvert.SerializeObject(Map, formatting, JsonSettings);
            File.WriteAllText(jsonPath, json);
        }

        public override List<ValidationResult> CanExport()
        {
            TreasuredSceneValidator validator = new TreasuredSceneValidator(Map);
            return validator.GetValidationResults();
        }
    }
}
