using UnityEngine;
using UnityEditor;
using Treasured.SDK;
using System.Linq;

namespace Treasured.SDKEditor
{
    [CustomEditor(typeof(Hotspot))]
    public class HotspotEditor : Editor
    {
        private static readonly string[] Excludes = new string[] { "m_Script" };

        private Hotspot _hotspot;

        private void OnEnable()
        {
            _hotspot = target as Hotspot;
            _hotspot.transform.eulerAngles = new Vector3(0, _hotspot.transform.eulerAngles.y, 0);
        }
        private void OnDisable()
        {
            _hotspot.transform.hideFlags = HideFlags.None;
            Tools.hidden = false; // show the transform tools for other game object
            if (_hotspot)
            {
                _hotspot.transform.eulerAngles = new Vector3(0, _hotspot.transform.eulerAngles.y, 0);
            }
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            SerializedProperty iterator = serializedObject.GetIterator();
            iterator.Next(true);
            while (iterator.NextVisible(false))
            {
                if (Excludes.Contains(iterator.name))
                {
                    continue;
                }
                EditorGUILayout.PropertyField(iterator);
            }
            serializedObject.ApplyModifiedProperties();
            using (new GUILayout.VerticalScope())
            {
                using (new GUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Offset Hitbox"))
                    {
                        if (Physics.Raycast(_hotspot.transform.position, _hotspot.transform.position - _hotspot.transform.position + Vector3.down, out RaycastHit hit, 100))
                        {
                            _hotspot.Hitbox.Center = hit.point + new Vector3(0, _hotspot.Hitbox.Size.y / 2, 0);
                        }
                    }
                }
                EditorGUILayout.LabelField("Preview", EditorStyles.largeLabel);
                using (new GUILayout.HorizontalScope())
                {
                    using (new EditorGUI.DisabledGroupScope(false))
                    {
                        if (GUILayout.Button("Previous"))
                        {
                            _hotspot.transform.GetPreviousSibling()?.MoveSceneViewAndSelect();
                        }
                    }
                    using (new EditorGUI.DisabledGroupScope(false))
                    {
                        if (GUILayout.Button("Current"))
                        {
                            _hotspot.transform.MoveSceneViewAndSelect();
                        }
                    }
                    using (new EditorGUI.DisabledGroupScope(false))
                    {
                        if (GUILayout.Button("Next"))
                        {
                            _hotspot.transform.GetNextSibling()?.MoveSceneViewAndSelect();
                        }
                    }
                }
            }
        }

        private void OnSceneGUI()
        {
            if (Tools.current == Tool.Rotate)
            {
                float size = HandleUtility.GetHandleSize(_hotspot.transform.position);
                Tools.hidden = true;
                Handles.color = Color.blue;
                Handles.ArrowHandleCap(0, _hotspot.transform.position + _hotspot.transform.forward * size, _hotspot.transform.rotation, size, EventType.Repaint);
                _hotspot.transform.rotation = Handles.Disc(_hotspot.transform.rotation, _hotspot.transform.position, Vector3.up, size, false, 2);
                HandleUtility.AddDefaultControl(0);
            }
            else
            {
                Tools.hidden = false;
            }
            Handles.color = Color.white;
            Handles.Label(_hotspot.transform.position + Vector3.down, new GUIContent($"[Hotspot] {_hotspot.Name}"));
        }
    }
}
