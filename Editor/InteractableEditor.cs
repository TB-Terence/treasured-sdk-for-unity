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
        private SerializedProperty actionGroups;

        private TreasuredMap map;

        private void OnEnable()
        {
            map = (target as Interactable).Map;

            id = serializedObject.FindProperty("_id");
            description = serializedObject.FindProperty("_description");
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
            if (targets.Length == 1)
            {
                list.OnGUI();
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
