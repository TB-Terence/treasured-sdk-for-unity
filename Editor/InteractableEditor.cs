using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [CustomEditor(typeof(Interactable))]
    [CanEditMultipleObjects]
    internal class InteractableEditor : UnityEditor.Editor
    {
        private ActionGroupListDrawer list;
        private SerializedProperty id;
        private SerializedProperty description;
        private SerializedProperty hitbox;
        private SerializedProperty actionGroups;

        private TreasuredMap map;

        private void OnEnable()
        {
            map = (target as Interactable).Map;
            (target as Interactable).TryInvokeMethods("OnSelectedInHierarchy");
            id = serializedObject.FindProperty("_id");
            description = serializedObject.FindProperty("_description");
            hitbox = serializedObject.FindProperty("_hitbox");
            actionGroups = serializedObject.FindProperty("_actionGroups");
            list = new ActionGroupListDrawer(serializedObject, actionGroups);
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
            EditorGUILayout.PropertyField(hitbox);
            if (targets.Length == 1)
            {
                list.OnGUI();
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
