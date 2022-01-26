using UnityEngine;
using UnityEditor;

namespace Treasured.UnitySdk
{
    [CustomEditor(typeof(VideoPlane))]
    internal class VideoPlaneEditor : UnityEditor.Editor
    {
        private void OnEnable()
        {
            Tools.hidden = true;
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
            SerializedProperty width = serializedObject.FindProperty("_width");
            SerializedProperty height = serializedObject.FindProperty("_height");
            SerializedProperty aspectRatio = serializedObject.FindProperty("_aspectRatio");
            SerializedProperty src = serializedObject.FindProperty("_src");

            EditorGUI.BeginChangeCheck();
            width.intValue = EditorGUILayout.IntField(new GUIContent(width.displayName, width.tooltip), Mathf.Clamp(width.intValue, 0, width.intValue));
            if (EditorGUI.EndChangeCheck())
            {
                if(aspectRatio.floatValue != 0)
                {
                    height.intValue = (int)(width.intValue / aspectRatio.floatValue);
                }
            }
            EditorGUI.BeginChangeCheck();
            height.intValue = EditorGUILayout.IntField(new GUIContent(height.displayName, height.tooltip), Mathf.Clamp(height.intValue, 0, height.intValue));
            if (EditorGUI.EndChangeCheck())
            {
                if (aspectRatio.floatValue != 0)
                {
                    width.intValue =(int)(height.intValue * aspectRatio.floatValue);
                }
            }
            aspectRatio.floatValue = EditorGUILayout.FloatField(new GUIContent(aspectRatio.displayName, aspectRatio.tooltip), Mathf.Clamp(aspectRatio.floatValue, 0, aspectRatio.floatValue));
            
            EditorGUILayout.PropertyField(src);

            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneViewGUI(SceneView scene)
        {
            if (target is VideoPlane videoPlane)
            {
                Transform transform = videoPlane.transform;
                switch (Tools.current)
                {
                    case Tool.Move:
                        EditorGUI.BeginChangeCheck();
                        Vector3 newPosition = Handles.PositionHandle(transform.position, transform.rotation);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(transform, "Move Video Plane Position");
                            transform.position = newPosition;
                        }
                        break;
                    case Tool.Rotate:
                        EditorGUI.BeginChangeCheck();
                        Quaternion newRotation = Handles.RotationHandle(transform.rotation, transform.position);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(transform, "Rotate Hotspot Camera");
                            transform.rotation = newRotation;
                        }
                        float size = HandleUtility.GetHandleSize(transform.transform.position);
                        Handles.color = Color.blue;
                        Handles.ArrowHandleCap(0, transform.position, transform.rotation, size, EventType.Repaint);
                        break;
                }
                Handles.color = Color.white;
                Matrix4x4 matrix = Handles.matrix;
                Handles.matrix = Matrix4x4.TRS(transform.position, transform.transform.rotation, Vector3.one);
                Handles.DrawWireCube(Vector3.zero, new Vector3(videoPlane.Width / 1000f * transform.localScale.x, videoPlane.Height / 1000f * transform.localScale.y, 0.01f));
                Handles.Label(Vector3.zero, videoPlane.Src);
                Handles.matrix = matrix;
            }
        }
    }
}
