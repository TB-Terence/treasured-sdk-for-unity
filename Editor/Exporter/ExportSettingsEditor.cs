using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [CustomEditor(typeof(ExportSettings))]
    internal class ExportSettingsEditor : UnityEditor.Editor
    {
        private static class Styles
        {
            public static readonly GUIContent labelOptimizeScene = new GUIContent("Optimize Scene", "Optimize the cubemaps and the scene.glb to decrease the final file size. Keep in mind that this will result in a longer export process.");
        }
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
                EditorGUILayout.Toggle(Styles.labelOptimizeScene, settings.optimizeScene);
            }
        }
    }
}
