using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Treasured.UnitySdk
{
    [CustomEditor(typeof(CubemapExporter))]
    internal class CubemapExporterEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            CubemapExporter exporter = (CubemapExporter)target;
            //EditorGUILayout.HelpBox(k_temporarySettings);
            SerializedProperty exportAllQualities = serializedObject.FindProperty(nameof(CubemapExporter.exportAllQualities));
            EditorGUILayout.PropertyField(exportAllQualities);
            if (!exportAllQualities.boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(CubemapExporter.imageQuality)));
            }
            serializedObject.ApplyModifiedProperties();
        }

        void OnPreferenceGUI(SerializedObject serializedObject)
        {
            this.OnInspectorGUI();
        }
    }
}