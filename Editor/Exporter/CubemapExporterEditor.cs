using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [CustomEditor(typeof(CubemapExporter))]
    internal class CubemapExporterEditor : UnityEditor.Editor
    {
        private static readonly GUIContent k_temporarySettings = new GUIContent("" +
            "The following settings will be apply during capture\n\n" +
            "<Global Volume Settings>\n\n" +
            "Exposure Mode -> Fixed\n\n" +
            "<Volume Settings>\n\n" +
            "Bloom > Intensity -> 0\n" +
            "Chromatic Aberration > Intensity -> 0\n" +
            "Depth of Field > Near/Far Blur Max Radius -> 0\n" +
            "Film grain > Intensity -> 0\n" +
            "Lens Distortion > Scale -> 0\n" +
            "Motion Blur > Intensity -> 0\n" +
            "Vignette > Intensity -> 0\n\n" +
            "<Camera>\n\n" +
            "Post Anti-Aliasing -> SMAA");
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.HelpBox(k_temporarySettings);
            SerializedProperty exportAllQualities = serializedObject.FindProperty(nameof(CubemapExporter.exportAllQualities));
            EditorGUILayout.PropertyField(exportAllQualities);
            if(!exportAllQualities.boolValue)
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
