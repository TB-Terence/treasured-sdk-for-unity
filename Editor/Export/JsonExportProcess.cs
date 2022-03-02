using Newtonsoft.Json;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace Treasured.UnitySdk
{
    [System.Serializable]
    public class JsonExportProcess : ExportProcess
    {
        public Formatting formatting = Formatting.Indented;
        public bool convertTransform = false;

        public static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ContractResolver = ContractResolver.Instance,
            CheckAdditionalContent = true
        };

        public override void OnGUI(string root, SerializedObject serializedObject)
        {
            formatting = (Formatting)EditorGUILayout.EnumPopup(new GUIContent("Formatting"), formatting);
            EditorGUI.BeginChangeCheck();
            ThreeJsTransformConverter.ShouldConvertToThreeJsTransform = EditorGUILayout.Toggle(new GUIContent("Convert To Three Js"), ThreeJsTransformConverter.ShouldConvertToThreeJsTransform);
            if (EditorGUI.EndChangeCheck())
            {
                ThreeJsTransformConverter.ShouldConvertToThreeJsTransform = convertTransform;
            }
        }

        public override void OnExport(string rootDirectory, TreasuredMap map)
        {
            string jsonPath = Path.Combine(rootDirectory, "data.json").Replace('/', '\\');
            string json = JsonConvert.SerializeObject(map, formatting, JsonSettings);
            File.WriteAllText(jsonPath, json);
        }
    }
}
