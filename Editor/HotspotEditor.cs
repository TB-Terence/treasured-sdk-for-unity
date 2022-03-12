using System.Collections.Generic;
using Treasured.UnitySdkEditor;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [CustomEditor(typeof(Hotspot))]
    [CanEditMultipleObjects]
    internal class HotspotEditor : UnityEditor.Editor
    {
        internal static class Styles
        {
            public static readonly GUIContent snapToGround = EditorGUIUtility.TrTextContent("Snap on ground", "Snap the object slightly above the ground from camera position. This also snap the first box collider to the ground based on the size.");
            public static readonly GUIContent missingMapComponent = EditorGUIUtility.TrTextContent("Missing Treasured Map Component in parent.", "", "Warning");
        }

        private static readonly GUIContent[] tabs = { new GUIContent("On Click"), new GUIContent("On Hover") };

        private ActionGroupListDrawer onClickList;
        private ActionGroupListDrawer onHoverList;
        private SerializedProperty id;
        private SerializedProperty icon;
        private SerializedProperty hitbox;
        private SerializedProperty camera;
        private SerializedProperty onClick;
        private SerializedProperty onHover;

        private TreasuredMap map;
        private SerializedObject serializedHitboxTransform;
        private SerializedObject serializedCameraTransform;

        private int selectedTabIndex;

        private bool showVisibleTargetsOnly;

        List<TreasuredObject> visibleTargets = new List<TreasuredObject>();


        private void OnEnable()
        {
            var hotspot = target as Hotspot;
            map = (target as Hotspot).Map;
            id = serializedObject.FindProperty("_id");
            icon = serializedObject.FindProperty("_icon");
            hitbox = serializedObject.FindProperty("_hitbox");
            camera = serializedObject.FindProperty("_camera");
            onClick = serializedObject.FindProperty("_onClick");
            onHover = serializedObject.FindProperty("_onHover");
            if (hotspot.Hitbox)
            {
                serializedHitboxTransform = new SerializedObject(hotspot.Hitbox.transform);
            }
            if (hotspot.Camera)
            {
                serializedCameraTransform = new SerializedObject(hotspot.Camera.transform);
            }
            if (serializedObject.targetObjects.Length == 1)
            {
                onClickList = new ActionGroupListDrawer(serializedObject, onClick);
                onHoverList = new ActionGroupListDrawer(serializedObject, onHover);
            }
            hotspot?.TryInvokeMethods("OnSelectedInHierarchy");
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
                EditorGUILayout.LabelField(Styles.missingMapComponent);
                return;
            }
            serializedObject.Update();
            if (serializedObject.targetObjects.Length == 1)
            {
                EditorGUILayout.PropertyField(id);
                EditorGUILayout.PropertyField(icon);
                EditorGUILayoutHelper.TransformPropertyField(serializedHitboxTransform, "Hitbox");
                EditorGUILayoutHelper.TransformPropertyField(serializedCameraTransform, "Camera", true, true, false);
                EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
                selectedTabIndex = GUILayout.SelectionGrid(selectedTabIndex, tabs, tabs.Length, TreasuredMapEditor.Styles.TabButton);
                switch (selectedTabIndex)
                {
                    case 0:
                        onClickList?.OnGUI();
                        break;
                    case 1:
                        onHoverList?.OnGUI();
                        break;
                }
            }
            if (GUILayout.Button(Styles.snapToGround, GUILayout.Height(24)))
            {
                foreach (var target in serializedObject.targetObjects)
                {
                    if (target is Hotspot hotspot)
                    {
                        hotspot.SnapToGround();
                    }
                }
            }
            EditorGUILayout.BeginFoldoutHeaderGroup(true, "Debug");
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUI.BeginChangeCheck();
                var newValue = EditorGUILayout.Toggle(new GUIContent("Show Visible Targets Only"), showVisibleTargetsOnly);
                if (EditorGUI.EndChangeCheck())
                {
                    showVisibleTargetsOnly = newValue;
                    if (newValue)
                    {
                        visibleTargets = (target as Hotspot).VisibleTargets;
                    }
                    SceneView.lastActiveSceneView.Repaint();
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneViewGUI(SceneView view)
        {
            if (target is Hotspot hotspot && hotspot.Hitbox != null && hotspot.Camera != null)
            {
                Transform cameraTransform = hotspot.Camera.transform;
                switch (Tools.current)
                {
                    case Tool.Move:
                        EditorGUI.BeginChangeCheck();
                        Vector3 newCameraPosition = Handles.PositionHandle(cameraTransform.position, cameraTransform.rotation);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(cameraTransform, "Move Hotspot Camera Position");
                            cameraTransform.position = newCameraPosition;
                        }
                        break;
                    case Tool.Rotate:
                        EditorGUI.BeginChangeCheck();
                        Quaternion newCameraRotation = Handles.RotationHandle(cameraTransform.rotation, cameraTransform.position);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(cameraTransform, "Rotate Hotspot Camera");
                            cameraTransform.rotation = newCameraRotation;
                        }
                        float size = HandleUtility.GetHandleSize(hotspot.transform.position);
                        Handles.color = Color.blue;
                        Handles.ArrowHandleCap(0, cameraTransform.position, cameraTransform.rotation, size, EventType.Repaint);
                        break;
                }
                Handles.color = Color.white;
                Handles.DrawDottedLine(hotspot.Hitbox.transform.position, hotspot.Camera.transform.position, 5);
                Matrix4x4 matrix = Handles.matrix;
                if (!showVisibleTargetsOnly)
                {
                    foreach (var obj in map.GetComponentsInChildren<TreasuredObject>())
                    {
                        if (obj == target || obj.Hitbox == null)
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
                        Handles.matrix = Matrix4x4.TRS(obj.Hitbox.transform.position, obj.Hitbox.transform.rotation, Vector3.one);
                        Handles.DrawWireCube(Vector3.zero, obj.Hitbox.transform.localScale);
                    }
                }
                else
                {
                    foreach (var target in visibleTargets)
                    {
                        if (target.Hitbox == null)
                        {
                            continue;
                        }
                        if (target is Hotspot)
                        {
                            Handles.color = new Color(1, 0, 0, 0.8f);
                        }
                        else if (target is Interactable)
                        {
                            Handles.color = new Color(0, 1, 0, 0.8f);
                        }
                        Handles.matrix = Matrix4x4.TRS(target.Hitbox.transform.position, target.Hitbox.transform.rotation, Vector3.one);
                        Handles.DrawWireCube(Vector3.zero, target.Hitbox.transform.localScale);
                    }
                }
                Handles.matrix = matrix;
            }
        }
    }
}
