﻿using Newtonsoft.Json;
using System.Collections.Generic;
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

        public override void Export()
        {
            string jsonPath = Path.Combine(Map.exportSettings?.OutputDirectory, "data.json").Replace('/', '\\');
            string json = JsonConvert.SerializeObject(Map, formatting, JsonSettings);
            File.WriteAllText(jsonPath, json);
        }

        public override List<ValidationResult> CanExport()
        {
            RequiredFieldValidator validator = new RequiredFieldValidator(Map);
            return validator.GetValidationResults();
        }
    }
}
