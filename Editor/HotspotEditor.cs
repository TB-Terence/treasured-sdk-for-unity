using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk.Editor
{
    [CustomEditor(typeof(Hotspot))]
    internal class HotspotEditor : TreasuredObjectEditor<Hotspot, HotspotData>
    {
        protected override void Init()
        {
            base.Init();
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
                    if (GUILayout.Button(new GUIContent("Place Hitbox on ground", "Put the Hitbox on the ground by doing a Raycast. The maximum distance for the Raycast is 100."), GUILayout.Height(24)))
                    {
                        Undo.RecordObject(Target.BoxCollider, "Offset Hitbox position");
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
