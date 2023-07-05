using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    public class RotationRecorder : EditorWindow
    {
        public static bool IsRecording { get; private set; }
        static SerializedProperty SerializedProperty;
        static bool movedToTarget;
        static Vector3 StartingPosition;

        public static void Start(SerializedProperty serializedProperty)
        {
            IsRecording = true;
            SceneView.duringSceneGui -= OnSceneView;
            SceneView.duringSceneGui += OnSceneView;
            if (serializedProperty != null)
            {
                SerializedProperty = serializedProperty;
                movedToTarget = false;
            }
        }

        public static void StartAtPosition(Vector3 startingPosition, SerializedProperty serializedProperty)
        {
            Start(serializedProperty);
            StartingPosition = startingPosition;
            SceneView.lastActiveSceneView.LookAt(startingPosition);
        }

        static void OnSceneView(SceneView sceneView)
        {
            if (IsRecording)
            {
                if (!movedToTarget)
                {
                    SceneView.lastActiveSceneView.ShowNotification(new GUIContent("Click anywhere to move to target position"));
                }
                else
                {
                    SceneView.lastActiveSceneView.ShowNotification(new GUIContent("Click anywhere on the scene to record rotation\nClick 'Stop Recording' to exit"));
                }
                Event e = Event.current;
                switch (e.type)
                {
                    case EventType.MouseDown:
                        if (e.button == 0)
                        {
                            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                            Quaternion lookDirection = Quaternion.LookRotation(ray.direction);
                            if (SerializedProperty.serializedObject.targetObject is Hotspot hotspot)
                            {
                                if (!movedToTarget)
                                {
                                    movedToTarget = true;
                                }
                                else
                                {
                                    SerializedProperty.quaternionValue = lookDirection;
                                    SerializedProperty.serializedObject.ApplyModifiedProperties();
                                }
                                SceneView.lastActiveSceneView.LookAt(hotspot.Camera.transform.position, lookDirection, 0.01f);
                            }
                        }
                        break;
                    case EventType.Layout:
                        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                        break;
                }
            }
        }

        public static void Complete()
        {
            IsRecording = false;
            SceneView.duringSceneGui -= OnSceneView;
            movedToTarget = false;
            if (!IsRecording) return;
        }
    }
}
