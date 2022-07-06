using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [CustomEditor(typeof(HTMLEmbed))]
    internal class HTMLEmbedEditor : UnityEditor.Editor
    {
        static readonly string[] ASPECT_RATIO_OPTIONS = { "16:9", "16:10" };

        private void OnEnable()
        {
            Tools.hidden = true;
            (target as TreasuredObject)?.TryInvokeMethods("OnSelectedInHierarchy");
            SceneView.duringSceneGui -= OnSceneViewGUI;
            SceneView.duringSceneGui += OnSceneViewGUI;
            if (target is HTMLEmbed htmlEmbed)
            {
                if (htmlEmbed.LockAspectRatio)
                {
                    ScaleByRatio();
                }
            }
        }

        private void OnDisable()
        {
            Tools.hidden = false;
            SceneView.duringSceneGui -= OnSceneViewGUI;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty lockAspectRatio = serializedObject.FindProperty("_lockAspectRatio");
            SerializedProperty aspectRatio = serializedObject.FindProperty("_aspectRatio");
            SerializedProperty src = serializedObject.FindProperty(nameof(HTMLEmbed.src));

            EditorGUILayout.PropertyField(src);
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(lockAspectRatio);
            if (EditorGUI.EndChangeCheck())
            {
                if (lockAspectRatio.boolValue)
                {
                    ScaleByRatio();
                }
            }
            if (lockAspectRatio.boolValue)
            {
                int index = ArrayUtility.IndexOf(ASPECT_RATIO_OPTIONS, aspectRatio.stringValue);
                EditorGUI.BeginChangeCheck();
                var newIndex = EditorGUILayout.Popup(new GUIContent("Aspect Ratio"), index, ASPECT_RATIO_OPTIONS);
                if (EditorGUI.EndChangeCheck())
                {
                    aspectRatio.stringValue = ASPECT_RATIO_OPTIONS[newIndex];
                    serializedObject.ApplyModifiedProperties();
                    ScaleByRatio();
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void ScaleByRatio()
        {
            if (!(serializedObject.targetObject is HTMLEmbed htmlEmbed))
            {
                return;
            }
            Transform transform = htmlEmbed.Hitbox?.transform;
            if (transform)
            {
                float ratio = htmlEmbed.AspectRatio;
                Undo.RecordObject(transform, "Scale HTML Embed Hitbox");
                transform.localScale = new Vector3(transform.localScale.x, transform.localScale.x / ratio, 0.01f);
            }
        }

        private void OnSceneViewGUI(SceneView scene)
        {
            if (target is HTMLEmbed htmlEmbed)
            {
                Transform transform = htmlEmbed.Hitbox.transform;
                switch (Tools.current)
                {
                    case Tool.Move:
                        EditorGUI.BeginChangeCheck();
                        Vector3 newPosition = Handles.PositionHandle(transform.position, transform.rotation);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(transform, "Move HTML Embed");
                            transform.position = newPosition;
                        }
                        break;
                    case Tool.Rotate:
                        EditorGUI.BeginChangeCheck();
                        Quaternion newRotation = Handles.RotationHandle(transform.rotation, transform.position);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(transform, "Rotate HTML Embed");
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
                            Undo.RecordObject(transform, "Scale HTML Embed Hitbox");
                            Vector2 scaleDelta = newScale - transform.localScale;
                            if (htmlEmbed.LockAspectRatio)
                            {
                                float ratio = htmlEmbed.AspectRatio;
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
