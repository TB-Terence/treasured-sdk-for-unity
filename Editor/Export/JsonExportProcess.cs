using Newtonsoft.Json;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace Treasured.UnitySdk
{
    internal class JsonExportProcess : ExportProcess
    {
        private Formatting formatting = Formatting.Indented;

        public static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ContractResolver = ContractResolver.Instance,
            CheckAdditionalContent = true
        };

        public override void OnGUI(SerializedObject serializedObject)
        {
            formatting = (Formatting)EditorGUILayout.EnumPopup(new GUIContent("Formatting"), formatting);
        }

        public override void Export(string rootDirectory, TreasuredMap map)
        {
            string jsonPath = Path.Combine(rootDirectory, "data.json").Replace('/', '\\');
            string json = JsonConvert.SerializeObject(map, formatting, JsonSettings);
            File.WriteAllText(jsonPath, json);
        }
    }
}
