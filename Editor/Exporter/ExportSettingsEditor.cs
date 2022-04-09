using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [CustomEditor(typeof(ExportSettings))]
    internal class ExportSettingsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            ExportSettings settings = target as ExportSettings;
            EditorGUILayout.LabelField("Export Settings", EditorStyles.boldLabel);
            using(new EditorGUI.IndentLevelScope(1))
            {
                EditorGUI.BeginChangeCheck();
                string newOutputFolderName = EditorGUILayout.TextField(new GUIContent("Folder Name"), settings.folderName);
                if (EditorGUI.EndChangeCheck() && !string.IsNullOrWhiteSpace(newOutputFolderName))
                {
                    settings.folderName = newOutputFolderName;
                }
            }
        }
    }
}
