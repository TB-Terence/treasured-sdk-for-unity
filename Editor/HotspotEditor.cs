using UnityEngine;
using UnityEditor;

namespace Treasured.UnitySdk
{
    [CustomEditor(typeof(Hotspot))]
    [CanEditMultipleObjects]
    internal class HotspotEditor : UnityEditor.Editor
    {
        internal static class Styles
        {
            public static readonly GUIContent snapToGround = EditorGUIUtility.TrTextContent("Snap on Ground", "Snap the object slightly above the ground from camera position. This also snap the first box collider to the ground based on the size.");
            public static readonly GUIContent missingMapComponent = EditorGUIUtility.TrTextContent("Missing Treasured Map Component in parent.", "", "Warning");
        }

        private static readonly Vector3 cameraCubeSize = Vector3.one * 0.3f;

        private ActionGroupListDrawer list;
        private SerializedProperty id;
        private SerializedProperty description;
        private SerializedProperty cameraPositionOffset;
        private SerializedProperty cameraRotationOffset;
        private SerializedProperty actionGroup;

        private TreasuredMap map;

        private void OnEnable()
        {
            map = (target as Hotspot).Map;

            id = serializedObject.FindProperty("_id");
            description = serializedObject.FindProperty("_description");
            cameraPositionOffset = serializedObject.FindProperty("_cameraPositionOffset");
            cameraRotationOffset = serializedObject.FindProperty("_cameraRotationOffset");
            actionGroup = serializedObject.FindProperty("_actionGroups");
            list = new ActionGroupListDrawer(serializedObject, actionGroup);
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
            if (!id.hasMultipleDifferentValues)
            {
                EditorGUILayout.PropertyField(id);
            }
            EditorGUILayout.PropertyField(description);
            EditorGUILayout.PropertyField(cameraPositionOffset);
            EditorGUILayout.PropertyField(cameraRotationOffset);
            if (serializedObject.targetObjects.Length == 1)
            {
                list.OnGUI();
            }
            serializedObject.ApplyModifiedProperties();
            if (GUILayout.Button("Snap to Ground", GUILayout.Height(24)))
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

        private void OnSceneViewGUI(SceneView view)
        {
            if (SceneView.lastActiveSceneView.size == 0.01f) // this happens when TreasuredObject is selected
            {
                return;
            }
            if (target is Hotspot hotspot)
            {
                TransformData cameraTransform = hotspot.CameraTransform;
                var cameraRotation = Quaternion.Euler(cameraTransform.Rotation);
                switch (Tools.current)
                {
                    case Tool.Move:
                        EditorGUI.BeginChangeCheck();
                        Vector3 newCameraPosition = Handles.PositionHandle(cameraTransform.Position, cameraRotation);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(target, "Edit hotpsot camera position offset");
                            cameraPositionOffset.vector3Value = newCameraPosition - hotspot.transform.position;
                            serializedObject.ApplyModifiedProperties();
                        }
                        break;
                    case Tool.Rotate:
                        EditorGUI.BeginChangeCheck();
                        Quaternion newRotation = Handles.RotationHandle(cameraRotation, cameraTransform.Position);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(hotspot.transform, "Edit hotspot camera rotation offset");
                            cameraRotationOffset.vector3Value = newRotation.eulerAngles - hotspot.transform.eulerAngles;
                            serializedObject.ApplyModifiedProperties();
                        }
                        float size = HandleUtility.GetHandleSize(hotspot.transform.position);
                        Handles.color = Color.blue;
                        Handles.ArrowHandleCap(0, cameraTransform.Position, cameraRotation, size, EventType.Repaint);
                        break;
                }
                Handles.color = Color.white;
                Handles.DrawDottedLine(hotspot.transform.position, cameraTransform.Position, 5);
                Handles.color = Color.red;
                Handles.DrawWireCube(cameraTransform.Position, cameraCubeSize);
            }
        }
    }
}
