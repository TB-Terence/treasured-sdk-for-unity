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

            SerializedProperty auidoContent = serializedObject.FindProperty(nameof(SoundSource.audioInfo));
            SerializedProperty distance = serializedObject.FindProperty("distance");

            EditorGUILayout.PropertyField(auidoContent);
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(distance);
            if (EditorGUI.EndChangeCheck())
            {
                var soundSource = serializedObject.targetObject as SoundSource;
                var hitBoxTransform = soundSource.Hitbox?.transform;
                if (hitBoxTransform)
                {
                    hitBoxTransform.localScale =
                        new Vector3(soundSource.distance, soundSource.distance, soundSource.distance);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
