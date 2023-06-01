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
            EditorGUILayoutUtils.FolderField(ref TreasuredSDKPreferences.Instance.customExportFolder, "Directory");
            EditorGUI.BeginChangeCheck();
            string newOutputFolderName = EditorGUILayout.TextField(new GUIContent("Folder Name"), folderName.stringValue);
            if (EditorGUI.EndChangeCheck() && !string.IsNullOrWhiteSpace(newOutputFolderName))
            {
                folderName.stringValue = newOutputFolderName.Trim();
            }
            EditorGUILayout.PropertyField(optimizeScene);
            SerializedProperty thumbnail = serializedObject.FindProperty(nameof(ExportSettings.thumbnail));
            EditorGUILayout.PropertyField(thumbnail);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
