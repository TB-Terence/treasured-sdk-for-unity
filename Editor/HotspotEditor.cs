﻿using UnityEngine;
using UnityEditor;

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

        private static readonly Vector3 cameraCubeSize = Vector3.one * 0.3f;

        private ActionGroupListDrawer list;
        private SerializedProperty id;
        private SerializedProperty description;
        private SerializedProperty hitbox;
        private SerializedProperty camera;
        private SerializedProperty actionGroup;

        private TreasuredMap map;

        private void OnEnable()
        {

            map = (target as Hotspot).Map;
            id = serializedObject.FindProperty("_id");
            description = serializedObject.FindProperty("_description");
            hitbox = serializedObject.FindProperty("_hitbox");
            camera = serializedObject.FindProperty("_camera");
            actionGroup = serializedObject.FindProperty("_actionGroups");
            if(serializedObject.targetObjects.Length == 1)
            {
                list = new ActionGroupListDrawer(serializedObject, actionGroup);
            }
            (target as Hotspot).CreateTransformGroupInternal();
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
                EditorGUILayout.PropertyField(description);
                EditorGUILayout.PropertyField(hitbox);
                EditorGUILayout.PropertyField(camera);
                list?.OnGUI();
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
            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneViewGUI(SceneView view)
        {
            if (SceneView.lastActiveSceneView.size == 0.01f) // this happens when TreasuredObject is selected
            {
                return;
            }
            if (target is Hotspot hotspot && hotspot.Hitbox != null && hotspot.Camera != null)
            {
                Transform cameraTransform = hotspot.Camera.transform;
                switch (Tools.current)
                {
                    case Tool.Move:
                        Vector3 newCameraPosition = Handles.PositionHandle(cameraTransform.position, cameraTransform.rotation);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(cameraTransform, "Move Hotspot Camera Position");
                            cameraTransform.position = newCameraPosition;
                        }
                        break;
                    case Tool.Rotate:
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
            }
        }
    }
}
