using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [CustomEditor(typeof(Interactable))]
    [CanEditMultipleObjects]
    internal class InteractableEditor : UnityEditor.Editor
    {
        private ActionBaseListDrawer list;
        private SerializedProperty id;
        private SerializedProperty onSelected;

        private TreasuredMap map;

        private void OnEnable()
        {
            map = (target as Interactable).Map;

            id = serializedObject.FindProperty("_id");
            onSelected = serializedObject.FindProperty("_onSelected");
            list = new ActionBaseListDrawer(serializedObject, onSelected);
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
                using (new EditorGUI.DisabledGroupScope(true))
                {
                    EditorGUILayout.PropertyField(id);
                }
            }
            if (!onSelected.hasMultipleDifferentValues)
            {
                list.OnGUI();
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
