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
        public static readonly GUIContent snapToGround = EditorGUIUtility.TrTextContent("Snap on ground", "Snap the object slightly above the ground from camera position. This also snap the first box collider to the ground based on the size.");

        private const string k_RecordingText = "Recording In Progress(Click on the scene to rotate the camera)...";

        private ActionGroupListDrawer onClickListDrawer;
        private SerializedProperty button;
        private SerializedProperty hitbox;
        private SerializedProperty _onClick;
        private SerializedProperty actionGraph;

        private SerializedObject serializedHitboxTransform;
        private SerializedObject serializedCameraTransform;

        private bool showVisibleTargetsOnly;

        List<TreasuredObject> visibleTargets = new List<TreasuredObject>();

        private bool isRecording;

        protected override void OnEnable()
        {
            base.OnEnable();
            var hotspot = target as Hotspot;
            button = serializedObject.FindProperty(nameof(TreasuredObject.button));
            hitbox = serializedObject.FindProperty("_hitbox");
            _onClick = serializedObject.FindProperty("_onClick");
            actionGraph = serializedObject.FindProperty(nameof(TreasuredObject.actionGraph));
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
                onClickListDrawer = new ActionGroupListDrawer(serializedObject, _onClick);
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
            base.OnInspectorGUI();
            serializedObject.Update();
            if (serializedObject.targetObjects.Length == 1)
            {
                EditorGUILayout.LabelField("Camera Rotation", EditorStyles.boldLabel);
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button(!isRecording ? "Start Recording" : "Stop Recording"))
                    {
                        isRecording = !isRecording;
                        SceneView.lastActiveSceneView.Repaint();
                    }
                    if (GUILayout.Button("Preview"))
                    {
                        (target as Hotspot).Camera.Preview();
                    }
                }
                EditorGUILayout.PropertyField(button);
                EditorGUILayoutUtils.TransformPropertyField(hitbox, "Hitbox");
                bool showDeprecatedActions = SessionState.GetBool(SessionKeys.ShowDeprecatedActions, false);
                SessionState.SetBool(SessionKeys.ShowDeprecatedActions, EditorGUILayout.ToggleLeft("Show Deprecated Actions", showDeprecatedActions));
                EditorGUI.BeginChangeCheck();
                bool isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(SessionState.GetBool(SessionKeys.ShowActionList, true), "Action Graph");
                if (EditorGUI.EndChangeCheck())
                {
                    SessionState.SetBool(SessionKeys.ShowActionList, isExpanded);
                }
                if (isExpanded)
                {
                    if (showDeprecatedActions)
                    {
                        onClickListDrawer?.OnGUI();
                    }
                    EditorGUILayout.PropertyField(actionGraph);
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
            }
            else
            {
                EditorGUILayout.HelpBox($"Multi-Editing is disabled.", MessageType.Info);
            }
            if (GUILayout.Button(snapToGround, GUILayout.Height(24)))
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

        private void OnSceneGUI()
        {
            if (isRecording)
            {
                Color previousColor = GUI.color;
                Handles.BeginGUI();
                GUI.color = Color.red;
                GUI.Label(new Rect(0, Screen.height / 2, Screen.width, EditorGUIUtility.singleLineHeight), k_RecordingText, GUIStyles.Instance["centeredLabel"]);
                GUI.color = previousColor;
                Handles.EndGUI();
                Event e = Event.current;
                switch (e.type)
                {
                    case EventType.MouseDown:
                        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                        if (e.button == 0)
                        {
                            Hotspot hotspot = target as Hotspot;
                            SetRotationFromDirection(hotspot, ray.direction);
                            hotspot.Camera.Preview();
                        }
                        break;
                    case EventType.Layout:
                        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                        break;
                }
            }
        }

        private void SetRotationFromDirection(Hotspot hotspot, Vector3 direction)
        {
            hotspot.Camera.transform.rotation = Quaternion.LookRotation(direction);
        }

        private void OnSceneViewGUI(SceneView view)
        {
            if (isRecording)
            {
                return;
            }
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
