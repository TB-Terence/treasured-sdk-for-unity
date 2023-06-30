using System.IO;
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
            serializedObject.Update();
            ExportSettings settings = target as ExportSettings;
            SerializedProperty folderName = serializedObject.FindProperty(nameof(ExportSettings.folderName));
            SerializedProperty optimizeScene = serializedObject.FindProperty(nameof(ExportSettings.optimizeScene));
            EditorGUILayoutUtils.FolderField(ref TreasuredSDKPreferences.Instance.customExportFolder, "Directory", "Change Output Directory");
            using(new EditorGUILayout.HorizontalScope())
            {
                EditorGUI.BeginChangeCheck();
                string newOutputFolderName = EditorGUILayout.TextField(new GUIContent("Folder Name"), folderName.stringValue);
                if (EditorGUI.EndChangeCheck() && !string.IsNullOrWhiteSpace(newOutputFolderName))
                {
                    folderName.stringValue = newOutputFolderName.Trim();
                }
                using (new EditorGUI.DisabledGroupScope(!Directory.Exists(settings.OutputDirectory)))
                {
                    if (GUILayout.Button(EditorGUIUtility.TrIconContent("SceneLoadIn", "Open Folder"), EditorStyles.label, GUILayout.Width(20)))
                    {
                        EditorUtility.OpenWithDefaultApp(settings.OutputDirectory);
                    }
                }
            }
            EditorGUILayout.PropertyField(optimizeScene);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
