using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [CustomEditor(typeof(CubemapExporter))]
    internal class CubemapExporterEditor : UnityEditor.Editor
    {
        private static readonly GUIContent k_temporarySettings = new GUIContent("" +
            "The following settings will be apply during capture\n\n" +
            "<Volume Settings>\n\n" +
            "Bloom > Intensity -> 0\n" +
            "Chromatic Aberration > Intensity -> 0\n" +
            "Depth of Field -> 0\n" +
            "Film grain > Intensity -> 0\n" +
            "Lens Distortion > Scale -> 0\n" +
            "Motion Blur > Intensity -> 0\n" +
            "Vignette > Intensity -> 0\n\n" +
            "<Camera>\n" +
            "Post Anti-Aliasing -> SMAA");

        private SerializedProperty _useCustomWidth;
        private SerializedProperty _customCubemapWidth;
        private SerializedProperty _imageQuality;
        private SerializedProperty _qualityPercentage;
        private SerializedProperty _exportAllQualities;

        private void OnEnable()
        {
            _useCustomWidth = serializedObject.FindProperty("_useCustomWidth");
            _customCubemapWidth = serializedObject.FindProperty("_customCubemapWidth");
            _imageQuality = serializedObject.FindProperty(nameof(CubemapExporter.imageQuality));
            _qualityPercentage = serializedObject.FindProperty("_qualityPercentage");
            _exportAllQualities = serializedObject.FindProperty(nameof(CubemapExporter.exportAllQualities));
        }

        public override void OnInspectorGUI()
        {
            CubemapExporter cubemapExportProcess = (CubemapExporter)target;
            EditorGUILayout.HelpBox(k_temporarySettings);
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(CubemapExporter.imageFormat)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(CubemapExporter.cubemapFormat)));
            _exportAllQualities.boolValue = EditorGUILayout.Toggle(new GUIContent("Export all Image Qualities"),
                _exportAllQualities.boolValue);

            if (!_exportAllQualities.boolValue)
            {
                EditorGUILayout.BeginHorizontal();
                if (_useCustomWidth.boolValue)
                {
                    _customCubemapWidth.intValue = EditorGUILayout.IntField(new GUIContent("Cubemap Width"), _customCubemapWidth.intValue);
                    if (cubemapExportProcess.cubemapFormat == CubemapFormat._3x2)
                    {
                        // Clamp value for 3x2 format
                        _customCubemapWidth.intValue = Mathf.Clamp(_customCubemapWidth.intValue - _customCubemapWidth.intValue % 10, 16, CubemapExporter.MAXIMUM_CUBEMAP_FACE_WIDTH);
                    }
                    else
                    {
                        _customCubemapWidth.intValue = Mathf.Clamp(_customCubemapWidth.intValue, 1, CubemapExporter.MAXIMUM_CUDA_TEXTURE_WIDTH);
                    }
                }
                else
                {
                    EditorGUILayout.PropertyField(_imageQuality);
                }
                float previousLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 1;
                _useCustomWidth.boolValue = EditorGUILayout.ToggleLeft(new GUIContent("Customize"), _useCustomWidth.boolValue);
                EditorGUIUtility.labelWidth = previousLabelWidth;
                EditorGUILayout.EndHorizontal();
            }

            if (cubemapExportProcess.imageFormat != ImageFormat.PNG && cubemapExportProcess.imageFormat != ImageFormat.Ktx2)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    _qualityPercentage.intValue = EditorGUILayout.IntSlider(new GUIContent("Quality Percentage"), _qualityPercentage.intValue, 1, 100);
                    GUILayout.Label("%");
                }
            }
        }

        void OnPreferenceGUI(SerializedObject serializedObject)
        {
            this.OnInspectorGUI();
        }
    }
}
