using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [CustomEditor(typeof(MeshExporter))]
    internal class MeshExporterEditor : UnityEditor.Editor
    {
        private SerializedProperty _includeTag;
        private SerializedProperty _excludeTag;
        private string[] _tagString;

        private SerializedProperty _keepCombinedMesh;
        private SerializedProperty _exportQuality;
        private SerializedProperty _displayLogs;

        private void OnEnable()
        {
            _includeTag = serializedObject.FindProperty(nameof(MeshExporter.includeTags));
            _excludeTag = serializedObject.FindProperty(nameof(MeshExporter.excludeTags));

            _keepCombinedMesh = serializedObject.FindProperty(nameof(MeshExporter.keepCombinedMesh));
            _exportQuality = serializedObject.FindProperty(nameof(MeshExporter.ExportQuality));
            _displayLogs = serializedObject.FindProperty(nameof(MeshExporter.displayLogs));
            _tagString = UnityEditorInternal.InternalEditorUtility.tags;
            MeshExporter.allTags = _tagString;
        }

        public override void OnInspectorGUI()
        {
            _includeTag.intValue = EditorGUILayout.MaskField("Include Tags", _includeTag.intValue, _tagString);
            _excludeTag.intValue = EditorGUILayout.MaskField("Exclude Tags", _excludeTag.intValue, _tagString);

            EditorGUILayout.PropertyField(_exportQuality);
            
            EditorGUILayout.PropertyField(_keepCombinedMesh);
            EditorGUILayout.PropertyField(_displayLogs);
        }

        void OnPreferenceGUI(SerializedObject serializedObject)
        {
            this.OnInspectorGUI();
        }
    }
}
