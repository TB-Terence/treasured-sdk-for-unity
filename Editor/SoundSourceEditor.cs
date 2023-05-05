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
            if (target is SoundSource soundSource)
            {
                soundSource.audioContent ??= new AudioInfo()
                {
                    volume = soundSource.Volume,
                    loop = soundSource.Loop,
                    remoteUri = soundSource.Src
                };
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty distance = serializedObject.FindProperty("Distance");
            SerializedProperty auidoContent = serializedObject.FindProperty(nameof(SoundSource.audioContent));

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
                        new Vector3(soundSource.Distance, soundSource.Distance, soundSource.Distance);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
