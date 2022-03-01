using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [ExportProcessSettings(EnabledByDefault = false, ShowInEditor = false)]
    internal class EmbedExportProcess : ExportProcess
    {
        public override void OnGUI(string root, SerializedObject serializedObject)
        {
            EditorGUILayout.BeginHorizontal();
            SerializedProperty generateStylesheet = serializedObject.FindProperty("generateStylesheet");
            EditorGUILayout.PropertyField(generateStylesheet);
            if (generateStylesheet.boolValue)
            {
                if (GUILayout.Button("Edit"))
                {
                    string stylePath = $"{root}/style.css";
                    if (!File.Exists(stylePath))
                    {
                        File.WriteAllText(stylePath, (serializedObject.targetObject as TreasuredMap).stylesheet);
                    }
                    using (Process process = new Process())
                    {
                        process.StartInfo.FileName = ScriptEditorUtility.GetExternalScriptEditor();
                        process.StartInfo.Arguments = $"\"{stylePath}\"";
                        process.Start();
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        public override void OnExport(string rootDirectory, TreasuredMap map)
        {
            var directoryInfo = CreateDirectory(rootDirectory);
            List<UnitySdk.CustomCodeAction> actions = GetActions<CustomCodeAction>(map.Hotspots);
            actions.AddRange(GetActions<CustomCodeAction>(map.Interactables));
            foreach (var action in actions)
            {
                CreateEmbedHTML(directoryInfo, action);
            }
            if (map.generateStylesheet)
            {
                string stylesheetPath = Path.Combine(rootDirectory, "style.css");
                File.WriteAllText(stylesheetPath, map.stylesheet);
            }
        }

        List<T> GetActions<T>(TreasuredObject[] objects) where T : UnitySdk.Action
        {
            List<T> actions = new List<T>();
            foreach (var obj in objects)
            {
                foreach (var actioinGroup in obj.OnClick)
                {
                    foreach (var action in actioinGroup.Actions)
                    {
                        if (action.GetType() == typeof(T))
                        {
                            actions.Add((T)action);
                        }
                    }
                }
                foreach (var actioinGroup in obj.OnHover)
                {
                    foreach (var action in actioinGroup.Actions)
                    {
                        if (action.GetType() == typeof(T))
                        {
                            actions.Add((T)action);
                        }
                    }
                }
            }
            return actions;
        }

        DirectoryInfo CreateDirectory(string rootDirectory)
        {
            DirectoryInfo directoryInfo = Directory.CreateDirectory(Path.Combine(rootDirectory, "Embed"));
            return directoryInfo;
        }

        void CreateEmbedHTML(DirectoryInfo directoryInfo, CustomCodeAction action)
        {
            string filePath = Path.Combine(directoryInfo.FullName, $"{action.Id}.html");
            File.WriteAllText(filePath, action.Code);
        }
    }
}
