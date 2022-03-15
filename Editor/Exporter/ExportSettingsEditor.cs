using System;
using System.IO;
using Treasured.UnitySdk;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [CustomEditor(typeof(ExportSettings))]
    internal class ExportSettingsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            ExportSettings settings = target as ExportSettings;
            EditorGUI.BeginChangeCheck();
            string newOutputFolderName = EditorGUILayout.TextField(new GUIContent("Output Folder Name"), settings.folderName);
            if (EditorGUI.EndChangeCheck() && !string.IsNullOrWhiteSpace(newOutputFolderName))
            {
                settings.folderName = newOutputFolderName;
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel(GUIContent.none);
                if (GUILayout.Button(new GUIContent("Open root folder", "Open the root output folder in the File Explorer. The default output root will be the path of your Unity project/Treasured Data"), GUILayout.Height(24)))
                {
                    Application.OpenURL(ExportSettings.DefaultOutputDirectory);
                }
                using (new EditorGUI.DisabledGroupScope(!Directory.Exists(settings.OutputDirectory)))
                {
                    if (GUILayout.Button(new GUIContent("Open current folder", "Open the current output folder in the File Explorer. This option is available after first export."), GUILayout.Height(24)))
                    {
                        Application.OpenURL(settings.OutputDirectory);
                    }
                }
            }
        }
    }
}
