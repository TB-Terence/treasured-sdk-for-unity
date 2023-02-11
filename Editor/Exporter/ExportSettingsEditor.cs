using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [CustomEditor(typeof(ExportSettings))]
    internal class ExportSettingsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            ExportSettings settings = target as ExportSettings;
            EditorGUILayout.LabelField("Export Settings", EditorStyles.boldLabel);
            using(new EditorGUI.IndentLevelScope(1))
            {
                SerializedProperty folderName = serializedObject.FindProperty(nameof(ExportSettings.folderName));
                SerializedProperty optimizeScene = serializedObject.FindProperty(nameof(ExportSettings.optimizeScene));
                EditorGUI.BeginChangeCheck();
                string newOutputFolderName = EditorGUILayout.TextField(new GUIContent("Folder Name"), folderName.stringValue);
                if (EditorGUI.EndChangeCheck() && !string.IsNullOrWhiteSpace(newOutputFolderName))
                {
                    folderName.stringValue = newOutputFolderName;
                }
                EditorGUILayout.PropertyField(optimizeScene);
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
