using System.Collections.Generic;
using System.Linq;
using Treasured.UnitySdk.Utilities;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [CustomEditor(typeof(Hotspot))]
    [CanEditMultipleObjects]
    [ComponentCard("Hotspot", "Hotspots are used to allow the user to navigate through the scene.", "Hotspot", "https://www.notion.so/treasured/Hotspots-aec47e5d3b59492cb2c00637baa1ead4")]
    internal class HotspotEditor : TreasuredObjectEditor
    {
        private ActionGroupListDrawer onClickListDrawer;
        private SerializedProperty button;
        private SerializedProperty hitbox;
        private SerializedProperty _onClick;
        private SerializedProperty actionGraph;

        private SerializedObject serializedCameraTransform;

        private bool showVisibleTargetsOnly;
        private Hotspot hotspot;

        protected override void OnEnable()
        {
            base.OnEnable();
            hotspot = target as Hotspot;
            button = serializedObject.FindProperty(nameof(TreasuredObject.icon));
            hitbox = serializedObject.FindProperty("_hitbox");
            _onClick = serializedObject.FindProperty("_onClick");
            actionGraph = serializedObject.FindProperty(nameof(TreasuredObject.actionGraph));
            if (hotspot.Camera)
            {
                serializedCameraTransform = new SerializedObject(hotspot.Camera.transform);
            }
            if (serializedObject.targetObjects.Length == 1)
            {
                onClickListDrawer = new ActionGroupListDrawer(serializedObject, _onClick);
            }
            hotspot?.TryInvokeMethods("OnSelectedInHierarchy");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();
            if (serializedObject.targetObjects.Length == 1)
            {
                EditorGUILayoutUtils.HitboxField(hotspot, true, false, false);
                EditorGUILayout.LabelField("Camera", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                EditorGUILayoutUtils.TransformField(hotspot.Camera.transform, true, true, false);
                if (GUILayout.Button("Preview"))
                {
                    EditorUtils.PreviewCamera(hotspot.Camera);
                }
                EditorGUI.indentLevel--;
                EditorGUILayout.PropertyField(button);
                EditorGUI.BeginChangeCheck();
                bool isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(SessionState.GetBool(SessionKeys.ShowActionList, true), "Action Graph");
                EditorGUILayout.EndFoldoutHeaderGroup();
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
                EditorGUILayout.HelpBox($"Multi-Editing for Action is disabled.", MessageType.Info);
            }
            //EditorGUILayout.BeginFoldoutHeaderGroup(true, "Debug");
            //using (new EditorGUI.IndentLevelScope())
            //{
            //    EditorGUI.BeginChangeCheck();
            //    var newValue = EditorGUILayout.Toggle(new GUIContent("Show Visible Targets Only"), showVisibleTargetsOnly);
            //    if (EditorGUI.EndChangeCheck())
            //    {
            //        showVisibleTargetsOnly = newValue;
            //        if (newValue)
            //        {
            //            visibleTargets = (target as Hotspot).VisibleTargets;
            //        }
            //        SceneView.lastActiveSceneView.Repaint();
            //    }
            //}
            //EditorGUILayout.EndFoldoutHeaderGroup();
            serializedObject.ApplyModifiedProperties();
        }

    }
}
