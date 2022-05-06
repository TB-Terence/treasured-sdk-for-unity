using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [CustomEditor(typeof(CubemapExporter))]
    internal class CubemapExporterEditor : UnityEditor.Editor
    {
        private bool _isAdvancedMode;
        private SerializedProperty _cubemapSize;
        private SerializedProperty _qualityPercentage;
        private SerializedProperty _exportAllQualities;

        private void OnEnable()
        {
            _cubemapSize = serializedObject.FindProperty("_cubemapSize");
            _qualityPercentage = serializedObject.FindProperty("_qualityPercentage");
            _exportAllQualities = serializedObject.FindProperty(nameof(CubemapExporter.exportAllQualities));
        }

        public override void OnInspectorGUI()
        {
            CubemapExporter cubemapExportProcess = (CubemapExporter)target;
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(CubemapExporter.imageFormat)));
            _exportAllQualities.boolValue = EditorGUILayout.Toggle(new GUIContent("Export all Image Qualities"),
                _exportAllQualities.boolValue);

            if (!_exportAllQualities.boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(CubemapExporter.imageQuality)));
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(CubemapExporter.cubemapFormat)));
            if (cubemapExportProcess.cubemapFormat == CubemapFormat._3x2)
            {
                _isAdvancedMode = EditorGUILayout.Toggle(new GUIContent("Advanced"), _isAdvancedMode);
                if (_isAdvancedMode)
                {
                    _cubemapSize.intValue = EditorGUILayout.IntField(new GUIContent("Cubemap Size"), _cubemapSize.intValue);
                    _cubemapSize.intValue = Mathf.Clamp(_cubemapSize.intValue - _cubemapSize.intValue % 10, 16, CubemapExporter.MAXIMUM_CUBEMAP_FACE_WIDTH);
                }
            }
            if (cubemapExportProcess.imageFormat == ImageFormat.PNG || cubemapExportProcess.imageFormat == ImageFormat.Ktx2)
                return;

            using (new EditorGUILayout.HorizontalScope())
            {
                _qualityPercentage.intValue = EditorGUILayout.IntSlider(new GUIContent("Quality Percentage"), _qualityPercentage.intValue, 1, 100);
                GUILayout.Label("%");
            }
        }
    }
}
