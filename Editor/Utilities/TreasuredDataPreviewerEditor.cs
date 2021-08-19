using UnityEngine;
using UnityEditor;
using Treasured.SDK;

namespace Treasured.SDKEditor
{
    [CustomEditor(typeof(TreasuredDataPreviewer))]
    public class TreasuredDataPreviewerEditor : Editor
    {
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
            var hotspots = _previewer.Data.Hotspots;
            
            Vector3 snap = Vector3.one * 0.5f;

            for (int i = 0; i < hotspots.Count; i++)
            {
                TreasuredObject current = hotspots[i];
                TreasuredObject next = hotspots[(i + 1) % hotspots.Count];

                Handles.color = Color.white;
                Handles.DrawWireCube(current.Transform.Position, Vector3.one * 0.1f);
                Handles.DrawWireCube(current.Hitbox.Center, current.Hitbox.Size);
                
                
                if (Tools.current == Tool.Move)
                {
                    EditorGUI.BeginChangeCheck();
                    Vector3 newPosition = Handles.FreeMoveHandle(current.Transform.Position, Quaternion.identity, 1, snap, Handles.RectangleHandleCap);
                    if (EditorGUI.EndChangeCheck())
                    {
                        current.Transform.Position = newPosition;
                    }
                }

                Handles.Label(current.Transform.Position + Vector3.down, new GUIContent($"[Hotspot] {current.Name}"));

                if (i == hotspots.Count - 1 && !_previewer.Data.Loop)
                {
                    continue;
                }
                Handles.DrawDottedLine(current.Transform.Position, next.Transform.Position, 5);
                Handles.color = Color.green;
                Vector3 direction = next.Transform.Position - current.Transform.Position;
                // draw multiple arrows
                //int segement = (int)(direction.magnitude / 3);
                //for (int x = 0; x < segement; x++)
                //{
                //    Handles.ArrowHandleCap(0, current.Position + direction.normalized * x * direction.magnitude / segement, Quaternion.LookRotation(direction), 1, EventType.Repaint);
                //}
                if (direction != Vector3.zero)
                {
                    Handles.ArrowHandleCap(0, current.Transform.Position, Quaternion.LookRotation(direction), 1, EventType.Repaint);
                }
                Handles.color = Color.red;
                Handles.ArrowHandleCap(0, current.Transform.Position, Quaternion.Euler(current.Transform.Rotation), 1, EventType.Repaint);
            }
            Handles.color = Color.white;
            var interactables = _previewer.Data.Interactables;
            for (int i = 0; i < interactables.Count; i++)
            {
                TreasuredObject current = interactables[i];
                Handles.DrawWireCube(current.Transform.Position, Vector3.one);

                if (Tools.current == Tool.Move)
                {
                    EditorGUI.BeginChangeCheck();
                    Vector3 newPosition = Handles.FreeMoveHandle(current.Transform.Position, Quaternion.identity, 1, snap, Handles.RectangleHandleCap);
                    if (EditorGUI.EndChangeCheck())
                    {
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
                        current.Transform.Rotation = newRotation.eulerAngles;
                    }
                }
                else if(Tools.current == Tool.Scale)
                {
                    EditorGUI.BeginChangeCheck();
                    Vector3 newSize = Handles.ScaleHandle(current.Hitbox.Size, current.Transform.Position, Quaternion.identity, 1);
                    if (EditorGUI.EndChangeCheck())
                    {
                        current.Hitbox.Size = newSize;
                    }
                    
                }
                Handles.Label(current.Transform.Position, current.Name);
            }
        }
    }
}
