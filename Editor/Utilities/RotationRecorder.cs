using System;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [InitializeOnLoad]
    public class RotationRecorder
    {
        public class Styles
        {
            public static readonly GUIContent recordOn = EditorGUIUtility.TrIconContent("Record On", "Stop Record Camera Rotation");
            public static readonly GUIContent recordOff = EditorGUIUtility.TrIconContent("Record Off", "Start Record Camera Rotation");
        }
        public static bool IsRecording { get; private set; }
        static Vector3 StartPosition;
        static Quaternion StartRotation;
        static Action<Quaternion> EndRotation;

        static RotationRecorder()
        {
            SceneView.duringSceneGui -= OnSceneView;
            EditorApplication.hierarchyChanged += Complete;
        }

        public static void Start(Vector3 startPosition, Quaternion startRotation, Action<Quaternion> endRotation)
        {
            IsRecording = true;
            SceneView.duringSceneGui -= OnSceneView;
            SceneView.duringSceneGui += OnSceneView;
            StartPosition = startPosition;
            StartRotation = startRotation;
            EndRotation = endRotation;
            LookAt(StartPosition, StartRotation);
        }

        static void OnSceneView(SceneView sceneView)
        {
            if (IsRecording)
            {
                SceneView.lastActiveSceneView.ShowNotification(new GUIContent("Click anywhere on the scene to record rotation\nClick 'Stop Recording' to exit"));
                Event e = Event.current;
                switch (e.type)
                {
                    case EventType.MouseDown:
                        if (e.button == 0)
                        {
                            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                            Quaternion lookDirection = Quaternion.LookRotation(ray.direction);
                            LookAt(StartPosition, lookDirection);
                            EndRotation.Invoke(lookDirection);
                        }
                        break;
                    case EventType.Layout:
                        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                        break;
                }
            }
        }

        static void LookAt(Vector3 postion, Quaternion rotation)
        {
            SceneView.lastActiveSceneView.orthographic = false;
            SceneView.lastActiveSceneView.LookAt(postion, rotation, 0.01f);
            SceneView.lastActiveSceneView.Repaint();
        }

        public static void Complete()
        {
            IsRecording = false;
            SceneView.duringSceneGui -= OnSceneView;
        }
    }
}
