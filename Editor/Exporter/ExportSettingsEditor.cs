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
                if (GUILayout.Button(new GUIContent("Show root folder", "Show the root output folder in the File Explorer"), GUILayout.Height(18)))
                {
                    Application.OpenURL(ExportSettings.DefaultOutputDirectory);
                }
                using (new EditorGUI.DisabledGroupScope(!Directory.Exists(settings.OutputDirectory)))
                {
                    if (GUILayout.Button(new GUIContent("Show output folder", "Show the output folder in the File Explorer"), GUILayout.Height(18)))
                    {
                        Application.OpenURL(settings.OutputDirectory);
                    }
                }
            }
        }
    }
}
