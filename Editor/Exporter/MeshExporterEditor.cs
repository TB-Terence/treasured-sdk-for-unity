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
        private SerializedProperty _canUseTag;
        private string[] _tagString;

        private SerializedProperty _filterLayerMask;
        private SerializedProperty _canUseLayerMask;

        private SerializedProperty _keepCombinedMesh;

        private void OnEnable()
        {
            _includeTag = serializedObject.FindProperty(nameof(MeshExporter.includeTags));
            _excludeTag = serializedObject.FindProperty(nameof(MeshExporter.excludeTags));
            _canUseTag = serializedObject.FindProperty(nameof(MeshExporter.canUseTag));

            _filterLayerMask = serializedObject.FindProperty(nameof(MeshExporter.filterLayerMask));
            _canUseLayerMask = serializedObject.FindProperty(nameof(MeshExporter.canUseLayerMask));

            _keepCombinedMesh = serializedObject.FindProperty(nameof(MeshExporter.keepCombinedMesh));
            
            //  TODO: Find a better way to add tags to the runtime scripts
            _tagString = UnityEditorInternal.InternalEditorUtility.tags;
            MeshExporter.allTags = _tagString;
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginHorizontal();
            _canUseTag.boolValue = EditorGUILayout.Toggle(
                new GUIContent("Use Tag", "Only combine GameObjects which this tag."),
                _canUseTag.boolValue);

            EditorGUILayout.BeginVertical();
            if (_canUseTag.boolValue)
            {
                _includeTag.intValue = EditorGUILayout.MaskField("Include Tags", _includeTag.intValue, _tagString);
                _excludeTag.intValue = EditorGUILayout.MaskField("Exclude Tags", _excludeTag.intValue, _tagString);
            }
            else
            {
                _includeTag.intValue = -1;
                _excludeTag.intValue = 0;
            }
            EditorGUILayout.EndVertical();

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
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(MeshExporter.ExportQuality)));
            }
        }
    }
}
