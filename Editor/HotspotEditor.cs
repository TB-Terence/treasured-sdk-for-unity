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
        private SerializedProperty cameraTransform;
        private SerializedProperty actionGroup;

        private TreasuredMap map;

        private void OnEnable()
        {
            map = (target as Hotspot).Map;

            id = serializedObject.FindProperty("_id");
            description = serializedObject.FindProperty("_description");
            cameraTransform = serializedObject.FindProperty("_cameraTransform");
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
            EditorGUILayout.PropertyField(cameraTransform);
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
                Handles.color = Color.white;
                Handles.DrawDottedLine(hotspot.transform.position, cameraTransform.Position, 5);
                Handles.color = Color.red;
                Handles.DrawWireCube(cameraTransform.Position, cameraCubeSize);
            }
        }
    }
}
