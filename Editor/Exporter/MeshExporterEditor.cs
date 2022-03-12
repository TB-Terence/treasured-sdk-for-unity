using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [CustomEditor(typeof(MeshExporter))]
    internal class MeshExporterEditor : Editor
    {
        private SerializedProperty _filterTag;
        private SerializedProperty _canUseTag;

        private SerializedProperty _filterLayerMask;
        private SerializedProperty _canUseLayerMask;

        private SerializedProperty _keepCombinedMesh;

        private void OnEnable()
        {
            _filterTag = serializedObject.FindProperty(nameof(MeshExporter.filterTag));
            _canUseTag = serializedObject.FindProperty(nameof(MeshExporter.canUseTag));

            _filterLayerMask = serializedObject.FindProperty(nameof(MeshExporter.filterLayerMask));
            _canUseLayerMask = serializedObject.FindProperty(nameof(MeshExporter.canUseLayerMask));

            _keepCombinedMesh = serializedObject.FindProperty(nameof(MeshExporter.keepCombinedMesh));
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginHorizontal();
            _canUseTag.boolValue = EditorGUILayout.Toggle(
                new GUIContent("Use Tag", "Only combine GameObjects which this tag."),
                _canUseTag.boolValue);

            if (_canUseTag.boolValue)
            {
                _filterTag.stringValue = EditorGUILayout.TagField("", _filterTag.stringValue);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            _canUseLayerMask.boolValue = EditorGUILayout.Toggle(
                new GUIContent("Use LayerMask",
                    "Only combine GameObjects which Layers is in this LayerMask."),
                _canUseLayerMask.boolValue);
            if (_canUseLayerMask.boolValue)
            {
                EditorGUILayout.PropertyField(_filterLayerMask, GUIContent.none);
            }

            EditorGUILayout.EndHorizontal();

            if (_canUseTag.boolValue || _canUseLayerMask.boolValue)
            {
                EditorGUILayout.PropertyField(_keepCombinedMesh);
            }
        }
    }
}
