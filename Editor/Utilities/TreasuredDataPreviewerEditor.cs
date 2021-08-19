using UnityEngine;
using UnityEditor;
using Treasured.SDK;

namespace Treasured.SDKEditor
{
    [CustomEditor(typeof(TreasuredDataPreviewer))]
    public class TreasuredDataPreviewerEditor : Editor
    {
        private static readonly Color HandleColor_Label = Color.red;
        private static readonly Color HandleColor_Object = Color.white;
        private static readonly Color HandleColor_Hitbox = Color.green;
        private static readonly Color HandleColor_Hotspot = Color.white;
        private static readonly Color HandleColor_HotspotNextArrow = Color.green;
        private static readonly Color HandleColor_HotspotNextLine = Color.green;
        private static readonly Color HandleColor_HotspotDirection = Color.red;

        private TreasuredDataPreviewer _previewer;

        private void OnEnable()
        {
            _previewer = target as TreasuredDataPreviewer;
        }

        // DO NOT REMOVE
        public override void OnInspectorGUI()
        {
            // Draw nothing in inspector
            //EditorGUILayout.HelpBox("This feature is currently in the Experimental stage.", MessageType.Info);
        }

        private void OnSceneGUI()
        {
            if(!_previewer.Data)
            {
                return;
            }
            Color previousHandleColor = Handles.color;

            var hotspots = _previewer.Data.Hotspots;
            
            Vector3 snap = Vector3.one * 0.5f;

            for (int i = 0; i < hotspots.Count; i++)
            {
                TreasuredObject current = hotspots[i];
                TreasuredObject next = hotspots[(i + 1) % hotspots.Count];

                Handles.color = HandleColor_Hotspot;
                Handles.DrawWireCube(current.Transform.Position, Vector3.one * 0.1f);
                Handles.color = Color.white;
                Handles.DrawDottedLine(current.Transform.Position, current.Hitbox.Center, 5);

                if (Tools.current == Tool.Move)
                {
                    EditorGUI.BeginChangeCheck();
                    Vector3 newPosition = Handles.FreeMoveHandle(current.Transform.Position, Quaternion.identity, 1, snap, Handles.CircleHandleCap);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(_previewer.Data, "Move hotspot");
                        current.Transform.Position = newPosition;
                    }
                }
                else if (Tools.current == Tool.Rotate)
                {
                    Quaternion newRotation = Quaternion.Euler(current.Transform.Rotation);
                    EditorGUI.BeginChangeCheck();
                    newRotation = Handles.FreeRotateHandle(newRotation, current.Transform.Position, 1);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(_previewer.Data, "Rotate hotspot camera");
                        current.Transform.Rotation = newRotation.eulerAngles;
                    }
                }
                Handles.color = HandleColor_Label;
                Handles.Label(current.Transform.Position + Vector3.down, new GUIContent($"[Hotspot] {current.Name}"));

                if (i == hotspots.Count - 1 && !_previewer.Data.Loop)
                {
                    continue;
                }
                Handles.color = HandleColor_HotspotNextLine;
                Handles.DrawLine(current.Transform.Position, next.Transform.Position, 1);
                Handles.color = HandleColor_HotspotNextArrow;
                Vector3 direction = next.Transform.Position - current.Transform.Position;
                // draw multiple arrows
                //int segement = (int)(direction.magnitude / 3);
                //for (int x = 0; x < segement; x++)
                //{
                //    Handles.ArrowHandleCap(0, current.Position + direction.normalized * x * direction.magnitude / segement, Quaternion.LookRotation(direction), 1, EventType.Repaint);
                //}
                
                Handles.color = HandleColor_HotspotDirection;
                Handles.ArrowHandleCap(0, current.Transform.Position, Quaternion.Euler(current.Transform.Rotation), 1, EventType.Repaint);
                Handles.color = HandleColor_Hitbox;
                Handles.DrawWireCube(current.Hitbox.Center, current.Hitbox.Size);
                if (direction != Vector3.zero)
                {
                    Handles.color = HandleColor_HotspotNextArrow;
                    Handles.ArrowHandleCap(0, current.Transform.Position, Quaternion.LookRotation(direction), 1, EventType.Repaint);
                }
            }
            Handles.color = Color.white;
            var interactables = _previewer.Data.Interactables;
            for (int i = 0; i < interactables.Count; i++)
            {
                TreasuredObject current = interactables[i];

                if (Tools.current == Tool.Move)
                {
                    EditorGUI.BeginChangeCheck();
                    Vector3 newPosition = Handles.FreeMoveHandle(current.Transform.Position, Quaternion.identity, 1, snap, Handles.RectangleHandleCap);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(_previewer.Data, "Move interactable");
                        current.Transform.Position = newPosition;
                    }
                }
                else if (Tools.current == Tool.Rotate)
                {
                    Quaternion newRotation = Quaternion.Euler(current.Transform.Rotation);
                    EditorGUI.BeginChangeCheck();
                    newRotation = Handles.FreeRotateHandle(newRotation, current.Transform.Position, 1);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(_previewer.Data, "Rotate interactable");
                        current.Transform.Rotation = newRotation.eulerAngles;
                    }
                }
                //else if(Tools.current == Tool.Scale)
                //{
                //    EditorGUI.BeginChangeCheck();
                //    Vector3 newSize = Handles.ScaleHandle(current.Hitbox.Size, current.Transform.Position, Quaternion.identity, 1);
                //    if (EditorGUI.EndChangeCheck())
                //    {
                //        Undo.RecordObject(_previewer.Data, "Scale interactable");
                //        current.Hitbox.Size = newSize;
                //    }
                    
                //}
                Handles.Label(current.Transform.Position + Vector3.down, new GUIContent($"[Interactable] {current.Name}"));
                Handles.color = HandleColor_Object;
                Handles.DrawWireCube(current.Transform.Position, Vector3.one);
            }
            Handles.color = previousHandleColor;
        }
    }
}
