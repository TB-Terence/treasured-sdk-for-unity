using System;
using System.Collections;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Treasured.UnitySdk
{

    [CustomEditor(typeof(TreasuredMap))]
    public class TreasuredMapEditor : Editor
    {
        private SerializedProperty _id;
        private SerializedProperty title;
        private SerializedProperty description;

        TreasuredMap map;

        private void OnEnable()
        {
            map = target as TreasuredMap;
            _id = serializedObject.FindProperty(nameof(_id));
            title = serializedObject.FindProperty(nameof(title));
            description = serializedObject.FindProperty(nameof(description));
            foreach (var obj in map.GetObjects())
            {
                obj.SetMap(map);
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(_id);
            EditorGUILayout.PropertyField(title);
            EditorGUILayout.PropertyField(description);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
