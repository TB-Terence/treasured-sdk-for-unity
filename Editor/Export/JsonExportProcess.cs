using Newtonsoft.Json;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace Treasured.UnitySdk
{
    internal class JsonExportProcess : ExportProcess
    {
        private static Formatting s_formatting = Formatting.Indented;
        
        public static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ContractResolver = ContractResolver.Instance,
            CheckAdditionalContent = true
        };

        public override void OnGUI(SerializedObject serializedObject)
        {
            s_formatting = (Formatting)EditorGUILayout.EnumPopup(new GUIContent("Formatting"), s_formatting);
            ThreeJsTransformConverter.ShouldConvertToThreeJsTransform = EditorGUILayout.Toggle(new GUIContent("Convert To Three Js"), ThreeJsTransformConverter.ShouldConvertToThreeJsTransform);
        }

        public override void OnExport(string rootDirectory, TreasuredMap map)
        {
            string jsonPath = Path.Combine(rootDirectory, "data.json").Replace('/', '\\');
            string json = JsonConvert.SerializeObject(map, s_formatting, JsonSettings);
            File.WriteAllText(jsonPath, json);
        }
    }
}
