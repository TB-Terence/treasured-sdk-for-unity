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

        public override void OnGUI(TreasuredMap map)
        {
            formatting = (Formatting)EditorGUILayout.EnumPopup(new GUIContent("Formatting"), formatting);
        }

        public override void Export(string rootDirectory, TreasuredMap map)
        {
            ValidateMap(map);
            string jsonPath = Path.Combine(rootDirectory, "data.json");
            string json = JsonConvert.SerializeObject(map, formatting, JsonSettings);
            File.WriteAllText(jsonPath, json);
        }                                               

        private void ValidateMap(TreasuredMap map)
        {
            if (string.IsNullOrWhiteSpace(map.Author))
            {
                throw new ContextException("Missing Field", "The author field is missing.", map);
            }
            if (string.IsNullOrWhiteSpace(map.Title))
            {
                throw new ContextException("Missing Field", "The title field is missing.", map);
            }
            if (string.IsNullOrWhiteSpace(map.Description))
            {
                throw new ContextException("Missing Field", "The description field is missing.", map);
            }
            foreach (var obj in map.GetComponentsInChildren<TreasuredObject>())
            {
                ValidateObject(obj);
            }
        }

        private void ValidateObject(TreasuredObject obj)
        {
            if (obj is Hotspot hotspot && hotspot.Camera == null)
            {
                throw new ContextException("Missing reference", $"Camera Transform is not assigned for {obj.name}.", obj);
            }
            foreach (var group in obj.OnClick)
            {
                foreach (var action in group.Actions)
                {
                    if (action is SelectObjectAction soa)
                    {
                        if (soa.Target == null || (soa.Target != null && !soa.Target.gameObject.activeSelf))
                        {
                            throw new ContextException("Missing reference", $"The target for Select-Object action is inactive OR is not assigned for {obj.name}.", obj);
                        }
                        else if (soa.Target.GetComponentInParent<TreasuredMap>() != obj.GetComponentInParent<TreasuredMap>())
                        {
                            throw new ContextException("Invalid reference", $"The target set for Select-Object action does not belong to the same map.", obj);
                        }
                    }
                }
            }
            foreach (var group in obj.OnHover)
            {
                foreach (var action in group.Actions)
                {
                    if (action is SelectObjectAction soa)
                    {
                        if (soa.Target == null || (soa.Target != null && !soa.Target.gameObject.activeSelf))
                        {
                            throw new ContextException("Missing reference", $"The target for OnHover-Object action is inactive OR is not assigned for {obj.name}.", obj);
                        }
                        else if (soa.Target.GetComponentInParent<TreasuredMap>() != obj.GetComponentInParent<TreasuredMap>())
                        {
                            throw new ContextException("Invalid reference", $"The target set for OnHover-Object action does not belong to the same map.", obj);
                        }
                    }
                }
            }
        }
    }
}
