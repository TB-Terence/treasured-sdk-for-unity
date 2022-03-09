using UnityEditor;

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

            EditorGUILayout.PropertyField(id);
            EditorGUILayout.PropertyField(src);
            EditorGUILayout.PropertyField(volume);
            EditorGUILayout.PropertyField(loop);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
