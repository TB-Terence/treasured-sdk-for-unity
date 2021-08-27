using UnityEngine;
using UnityEditor;
using Treasured.SDK;
using System.Linq;

namespace Treasured.SDKEditor
{
    [CustomEditor(typeof(Hotspot))]
    internal class HotspotEditor : TreasuredEditor<Hotspot>
    {
        protected override void Init()
        {
            Target.transform.eulerAngles = new Vector3(0, Target.transform.eulerAngles.y, 0);
        }
        private void OnDisable()
        {
            if (Target)
            {
                Target.transform.hideFlags = HideFlags.None;
            }
            Tools.hidden = false; // show the transform tools for other game object
            //Target.transform.eulerAngles = new Vector3(0, Target.transform.eulerAngles.y, 0);
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
                //EditorGUILayout.LabelField("Preview", EditorStyles.largeLabel);
                //using (new GUILayout.HorizontalScope())
                //{
                //    using (new EditorGUI.DisabledGroupScope(false))
                //    {
                //        if (GUILayout.Button("Previous"))
                //        {
                //            Target.transform.GetPreviousSibling()?.MoveSceneViewAndSelect();
                //        }
                //    }
                //    using (new EditorGUI.DisabledGroupScope(false))
                //    {
                //        if (GUILayout.Button("Current"))
                //        {
                //            Target.transform.MoveSceneViewAndSelect();
                //        }
                //    }
                //    using (new EditorGUI.DisabledGroupScope(false))
                //    {
                //        if (GUILayout.Button("Next"))
                //        {
                //            Target.transform.GetNextSibling()?.MoveSceneViewAndSelect();
                //        }
                //    }
                //}
            }
        }

        protected override void OnSceneGUI()
        {
            base.OnSceneGUI();
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
