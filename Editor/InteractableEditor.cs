using Treasured.Actions;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [CustomEditor(typeof(Interactable))]
    [CanEditMultipleObjects]
    internal class InteractableEditor : UnityEditor.Editor
    {
        private ActionGroupListDrawer onClickListDrawer;
        private SerializedProperty button;
        private SerializedProperty hitbox;
        private SerializedProperty onSelect;

        private TreasuredMap map;
        private SerializedObject serializedHitboxTransform;

        private void OnEnable()
        {
            map = (target as Interactable).Map;
            (target as Interactable).TryInvokeMethods("OnSelectedInHierarchy");
            button = serializedObject.FindProperty(nameof(TreasuredObject.button));
            hitbox = serializedObject.FindProperty("_hitbox");
            serializedHitboxTransform = new SerializedObject((target as Interactable).Hitbox.transform);
            onClickListDrawer = new ActionGroupListDrawer(serializedObject, serializedObject.FindProperty("_onClick"));
            onSelect = serializedObject.FindProperty(nameof(TreasuredObject.actionGraph)).FindPropertyRelative(nameof(ActionGraph.onSelect));
            SceneView.duringSceneGui -= OnSceneViewGUI;
            SceneView.duringSceneGui += OnSceneViewGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneViewGUI;
        }

        public override void OnInspectorGUI()
        {
            if (map == null)
            {
                EditorGUILayout.LabelField(HotspotEditor.Styles.missingMapComponent);
                return;
            }
            serializedObject.Update();
            EditorGUILayout.PropertyField(button);
            EditorGUILayoutUtils.ComponentTransformPropertyField(hitbox, serializedHitboxTransform, "Hitbox");
            if (targets.Length == 1)
            {
                bool showDeprecatedActions = SessionState.GetBool(SessionKeys.ShowDeprecatedActions, false);
                SessionState.SetBool(SessionKeys.ShowDeprecatedActions, EditorGUILayout.ToggleLeft("Show Deprecated Actions", showDeprecatedActions));
                EditorGUI.BeginChangeCheck();
                bool isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(SessionState.GetBool(SessionKeys.ShowActionList, true), "Action Graph");
                if (EditorGUI.EndChangeCheck())
                {
                    SessionState.SetBool(SessionKeys.ShowActionList, isExpanded);
                }
                if (isExpanded)
                {
                    if (showDeprecatedActions)
                    {
                        onClickListDrawer?.OnGUI();
                    }
                    EditorGUILayout.PropertyField(onSelect);
                }
            }
            else
            {
                EditorGUILayout.HelpBox($"Multi-Editing for Actions is disabled.", MessageType.None);
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneViewGUI(SceneView view)
        {
            if (target is Interactable interactable && interactable.Hitbox != null)
            {
                Matrix4x4 matrix = Handles.matrix;
                foreach (var obj in map.GetComponentsInChildren<TreasuredObject>())
                {
                    if (obj == target)
                    {
                        continue;
                    }
                    if (obj is Hotspot)
                    {
                        Handles.color = new Color(1, 0, 0, 0.8f);
                    }
                    else if (obj is Interactable)
                    {
                        Handles.color = new Color(0, 1, 0, 0.8f);
                    }
                    if (obj.Hitbox == null)
                    {
                        continue;
                    }
                    Handles.matrix = Matrix4x4.TRS(obj.Hitbox.transform.position, obj.Hitbox.transform.rotation, Vector3.one);
                    Handles.DrawWireCube(Vector3.zero, obj.Hitbox.transform.localScale);
                }
                Handles.matrix = matrix;
            }
        }
    }
}
