using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk.Editor
{
    [CustomEditor(typeof(Hotspot))]
    internal class HotspotEditor : TreasuredEditor<Hotspot>
    {
        protected override void Init()
        {
            Target.transform.eulerAngles = new Vector3(0, Target.transform.eulerAngles.y, 0);
            InjectDrawerAfter("_id", DrawNameField);
        }

        private void OnDisable()
        {
            if (Target)
            {
                Target.transform.hideFlags = HideFlags.None;
            }
            Tools.hidden = false; // show the transform tools for other game object
        }

        private void DrawNameField()
        {
            Target.gameObject.name = EditorGUILayout.TextField("Name", Target.gameObject.name);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            using (new GUILayout.VerticalScope())
            {
                using (new GUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Offset from ground"))
                    {
                        Target.OffsetHitbox();
                    }
                }
            }
        }

        protected override void OnSceneGUI()
        {
            base.OnSceneGUI();
            Handles.color = Color.red;
            Handles.DrawWireCube(Target.transform.position, Vector3.one * 0.3f);

            if (Tools.current == Tool.Scale || Tools.current == Tool.Transform || Tools.current == Tool.Rect)
            {
                Tools.hidden = true;
            }
            else
            {
                Tools.hidden = false;
            }
            if (Tools.current == Tool.Rotate)
            {
                float size = HandleUtility.GetHandleSize(Target.transform.position);
                Handles.color = Color.blue;
                Handles.ArrowHandleCap(0, Target.transform.position + Target.transform.forward * size, Target.transform.rotation, size, EventType.Repaint);
            }
        }
    }
}
