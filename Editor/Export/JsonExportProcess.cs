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
            TransformConverter.ConvertToThreeJsSpace = EditorGUILayout.Toggle(new GUIContent("Convert Transform"), TransformConverter.ConvertToThreeJsSpace);
        }

        public override void OnExport(string rootDirectory, TreasuredMap map)
        {
            string jsonPath = Path.Combine(rootDirectory, "data.json").Replace('/', '\\');
            string json = JsonConvert.SerializeObject(map, formatting, JsonSettings);
            File.WriteAllText(jsonPath, json);
        }
    }
}
