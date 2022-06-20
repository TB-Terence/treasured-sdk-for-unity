using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [CustomEditor(typeof(SoundSource))]
    internal class SoundSourceEditor : UnityEditor.Editor
    {
        private void OnEnable()
        {
            (target as TreasuredObject)?.TryInvokeMethods("OnSelectedInHierarchy");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty id = serializedObject.FindProperty("_id");
            SerializedProperty src = serializedObject.FindProperty("Src");
            SerializedProperty volume = serializedObject.FindProperty("Volume");
            SerializedProperty loop = serializedObject.FindProperty("Loop");
            SerializedProperty distance = serializedObject.FindProperty("Distance");

            EditorGUILayout.PropertyField(id);
            EditorGUILayout.PropertyField(src);
            EditorGUILayout.PropertyField(volume);
            EditorGUILayout.PropertyField(loop);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(distance);
            if (EditorGUI.EndChangeCheck())
            {
                var soundSource = serializedObject.targetObject as SoundSource;
                var hitBoxTransform = soundSource.Hitbox?.transform;
                if (hitBoxTransform)
                {
                    hitBoxTransform.localScale =
                        new Vector3(soundSource.Distance, soundSource.Distance, soundSource.Distance);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
