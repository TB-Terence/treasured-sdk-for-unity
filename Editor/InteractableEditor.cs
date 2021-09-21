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
            map = (target as Interactable).GetComponentInParent<TreasuredMap>();

            id = serializedObject.FindProperty("_id");
            onSelected = serializedObject.FindProperty("onSelected");
            list = new ActionBaseListDrawer(serializedObject, onSelected);
        }

        public override void OnInspectorGUI()
        {
            if (serializedObject.targetObjects.Length == 1 && GUILayout.Button("Select Map"))
            {
                if (map)
                {
                    Selection.activeGameObject = map.gameObject;
                }
            }
            serializedObject.Update();
            if (!id.hasMultipleDifferentValues)
            {
                EditorGUILayout.PropertyField(id);
            }
            if (!onSelected.hasMultipleDifferentValues)
            {
                list.OnGUI();
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
