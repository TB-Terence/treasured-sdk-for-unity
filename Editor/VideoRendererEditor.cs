﻿using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Treasured.UnitySdk
{
    [CustomEditor(typeof(VideoRenderer))]
    internal class VideoRendererEditor : UnityEditor.Editor
    {
        static readonly string[] ASPECT_RATIO_OPTIONS = { "16:9", "16:10" };

        private void OnEnable()
        {
            Tools.hidden = true;
            (target as TreasuredObject)?.TryInvokeMethods("OnSelectedInHierarchy");
            SceneView.duringSceneGui -= OnSceneViewGUI;
            SceneView.duringSceneGui += OnSceneViewGUI;
        }

        private void OnDisable()
        {
            Tools.hidden = false;
            SceneView.duringSceneGui -= OnSceneViewGUI;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty id = serializedObject.FindProperty("_id");
            SerializedProperty description = serializedObject.FindProperty("_description");
            SerializedProperty lockAspectRatio = serializedObject.FindProperty("_lockAspectRatio");
            SerializedProperty aspectRatio = serializedObject.FindProperty("_aspectRatio");
            SerializedProperty src = serializedObject.FindProperty("_src");

            EditorGUILayout.PropertyField(id);
            EditorGUILayout.PropertyField(description);
            EditorGUILayout.PropertyField(src);
            EditorGUILayout.PropertyField(lockAspectRatio);
            if (lockAspectRatio.boolValue)
            {
                int index = ArrayUtility.IndexOf(ASPECT_RATIO_OPTIONS, aspectRatio.stringValue);
                EditorGUI.BeginChangeCheck();
                var newIndex = EditorGUILayout.Popup(new GUIContent("Aspect Ratio"), index, ASPECT_RATIO_OPTIONS);
                if (EditorGUI.EndChangeCheck())
                {
                    aspectRatio.stringValue = ASPECT_RATIO_OPTIONS[newIndex];
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneViewGUI(SceneView scene)
        {
            if (target is VideoRenderer videoPlane)
            {
                Transform transform = videoPlane.Hitbox.transform;
                switch (Tools.current)
                {
                    case Tool.Move:
                        EditorGUI.BeginChangeCheck();
                        Vector3 newPosition = Handles.PositionHandle(transform.position, transform.rotation);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(transform, "Move Video Plane");
                            transform.position = newPosition;
                        }
                        break;
                    case Tool.Rotate:
                        EditorGUI.BeginChangeCheck();
                        Quaternion newRotation = Handles.RotationHandle(transform.rotation, transform.position);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(transform, "Rotate Video Plane");
                            transform.rotation = newRotation;
                        }
                        float size = HandleUtility.GetHandleSize(transform.transform.position);
                        Handles.color = Color.blue;
                        Handles.ArrowHandleCap(0, transform.position, transform.rotation, size, EventType.Repaint);
                        break;
                    case Tool.Scale:
                        EditorGUI.BeginChangeCheck();
                        Vector3 newScale = Handles.ScaleHandle(transform.localScale, transform.position, transform.rotation, HandleUtility.GetHandleSize(transform.position));
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(transform, "Scale Video Plane Hitbox");
                            Vector2 scaleDelta = newScale - transform.localScale;
                            if (videoPlane.LockAspectRatio)
                            {
                                float ratio = videoPlane.AspectRatio;
                                if (scaleDelta.x != 0)
                                {
                                    transform.localScale = new Vector3(newScale.x, newScale.x / ratio, 0.01f);
                                }
                                else if (scaleDelta.y != 0)
                                {
                                    transform.localScale = new Vector3(newScale.y * ratio, newScale.y, 0.01f);
                                }
                            }
                            else
                            {
                                transform.localScale = new Vector3(newScale.x, newScale.y, 0.01f);
                            }
                        }
                        break;
                }
            }
        }
    }
}
