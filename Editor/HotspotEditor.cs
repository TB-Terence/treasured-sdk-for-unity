using UnityEngine;
using UnityEditor;

namespace Treasured.UnitySdk
{
    [CustomEditor(typeof(Hotspot))]
    [CanEditMultipleObjects]
    internal class HotspotEditor : UnityEditor.Editor
    {
        static class Styles
        {
            public static readonly GUIContent snapToGround = EditorGUIUtility.TrTextContent("Snap on Ground", "Snap the object slightly above the ground from camera position. This also snap the first box collider to the ground based on the size.");
        }
        private static readonly Vector3 cameraCubeSize = Vector3.one * 0.3f;

        private ActionBaseListDrawer list;
        private SerializedProperty id;
        private SerializedProperty cameraPositionOffset;
        private SerializedProperty onSelected;

        private TreasuredMap map;

        private void OnEnable()
        {
            map = (target as Hotspot).Map;

            id = serializedObject.FindProperty("_id");
            cameraPositionOffset = serializedObject.FindProperty("_cameraPositionOffset");
            onSelected = serializedObject.FindProperty("_onSelected");
            list = new ActionBaseListDrawer(serializedObject, onSelected);
            SceneView.duringSceneGui -= OnSceneViewGUI;
            SceneView.duringSceneGui += OnSceneViewGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneViewGUI;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            if (!id.hasMultipleDifferentValues)
            {
                using (new EditorGUI.DisabledGroupScope(true))
                {
                    EditorGUILayout.PropertyField(id);
                }
            }
            EditorGUILayout.PropertyField(cameraPositionOffset);
            if (serializedObject.targetObjects.Length == 1)
            {
                list.OnGUI();
            }
            serializedObject.ApplyModifiedProperties();
            if (GUILayout.Button("Snap to Ground"))
            {
                foreach (var target in serializedObject.targetObjects)
                {
                    if (target is Hotspot hotspot)
                    {
                        hotspot.SnapToGround();
                    }
                }
            }
        }

        void OnSceneViewGUI(SceneView view)
        {
            if (SceneView.lastActiveSceneView.size == 0.01f)
            {
                return;
            }
            if (target is Hotspot hotspot)
            {
                Vector3 cameraPosition = hotspot.transform.position + cameraPositionOffset.vector3Value;
                switch (Tools.current)
                {
                    case Tool.Move:
                        EditorGUI.BeginChangeCheck();
                        Vector3 newCameraPosition = Handles.PositionHandle(cameraPosition, hotspot.transform.rotation);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(target, "Move hotpsot camera position");
                            cameraPositionOffset.vector3Value = newCameraPosition - hotspot.transform.position;
                            serializedObject.ApplyModifiedProperties();
                        }
                        break;
                    case Tool.Rotate:
                        EditorGUI.BeginChangeCheck();
                        Quaternion newRotation = Handles.RotationHandle(hotspot.transform.rotation, hotspot.transform.position + cameraPositionOffset.vector3Value);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(hotspot.transform, "Edit transform Rotation");
                            hotspot.transform.eulerAngles = newRotation.eulerAngles;
                        }
                        float size = HandleUtility.GetHandleSize(hotspot.transform.position);
                        Handles.color = Color.blue;
                        Handles.ArrowHandleCap(0, cameraPosition, hotspot.transform.rotation, size, EventType.Repaint);
                        break;
                }
                Handles.color = Color.white;
                Handles.DrawDottedLine(hotspot.transform.position, cameraPosition, 1);
                Handles.color = Color.red;
                Handles.DrawWireCube(cameraPosition, cameraCubeSize);
            }
        }
    }
}
