using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [CustomEditor(typeof(Interactable))]
    [CanEditMultipleObjects]
    [ComponentCard("Interactable", "Interactables are used to allow the user to click to interact with the scene in the browser.", "", "https://www.notion.so/treasured/Interactables-b34a557d38fa41af94062bfd6bd48fc3")]
    internal class InteractableEditor : TreasuredObjectEditor
    {
        private ActionGroupListDrawer onClickListDrawer;
        private SerializedProperty button;
        private SerializedProperty hitbox;
        private SerializedProperty actionGraph;

        private SerializedObject serializedHitboxTransform;

        protected override void OnEnable()
        {
            base.OnEnable();
            (target as Interactable).TryInvokeMethods("OnSelectedInHierarchy");
            button = serializedObject.FindProperty(nameof(TreasuredObject.icon));
            hitbox = serializedObject.FindProperty("_hitbox");
            serializedHitboxTransform = new SerializedObject((target as Interactable).Hitbox.transform);
            onClickListDrawer = new ActionGroupListDrawer(serializedObject, serializedObject.FindProperty("_onClick"));
            actionGraph = serializedObject.FindProperty(nameof(TreasuredObject.actionGraph));
            SceneView.duringSceneGui -= OnSceneViewGUI;
            SceneView.duringSceneGui += OnSceneViewGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneViewGUI;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();
            EditorGUILayout.PropertyField(button);
            EditorGUILayoutUtils.TransformPropertyField(hitbox, "Hitbox");
            if (targets.Length == 1)
            {
                EditorGUI.BeginChangeCheck();
                bool isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(SessionState.GetBool(SessionKeys.ShowActionList, true), "Action Graph");
                if (EditorGUI.EndChangeCheck())
                {
                    SessionState.SetBool(SessionKeys.ShowActionList, isExpanded);
                }
                if (isExpanded)
                {
                    EditorGUILayout.PropertyField(actionGraph);
                }
            }
            else
            {
                EditorGUILayout.HelpBox($"Multi-Editing for Action is disabled.", MessageType.None);
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneViewGUI(SceneView view)
        {
            return;
            if (target is Interactable interactable && interactable.Hitbox != null)
            {
                Matrix4x4 matrix = Handles.matrix;
                foreach (var obj in scene.GetComponentsInChildren<TreasuredObject>())
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
