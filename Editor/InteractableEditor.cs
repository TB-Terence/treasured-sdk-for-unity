using UnityEditor;

namespace Treasured.UnitySdk
{
    [CustomEditor(typeof(Interactable))]
    [CanEditMultipleObjects]
    internal class InteractableEditor : UnityEditor.Editor
    {
        private ActionGroupListDrawer onClickList;
        private SerializedProperty id;
        private SerializedProperty description;
        private SerializedProperty icon;
        private SerializedProperty hitbox;
        private SerializedProperty onClick;

        private TreasuredMap map;
        private SerializedObject serializedHitboxTransform;

        private void OnEnable()
        {
            map = (target as Interactable).Map;
            (target as Interactable).TryInvokeMethods("OnSelectedInHierarchy");
            id = serializedObject.FindProperty("_id");
            description = serializedObject.FindProperty("_description");
            icon = serializedObject.FindProperty("_icon");
            hitbox = serializedObject.FindProperty("_hitbox");
            serializedHitboxTransform = new SerializedObject((target as Interactable).Hitbox.transform);
            onClick = serializedObject.FindProperty("_onClick");
            onClickList = new ActionGroupListDrawer(serializedObject, onClick);
        }

        public override void OnInspectorGUI()
        {
            if (map == null)
            {
                EditorGUILayout.LabelField(HotspotEditor.Styles.missingMapComponent);
                return;
            }
            serializedObject.Update();
            if (!id.hasMultipleDifferentValues)
            {
                EditorGUILayout.PropertyField(id);
            }
            EditorGUILayout.PropertyField(description);
            EditorGUILayout.PropertyField(icon);
            EditorGUILayoutHelper.TransformPropertyField(serializedHitboxTransform, "Hitbox");
            if (targets.Length == 1)
            {
                onClickList.OnGUI(true);
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
