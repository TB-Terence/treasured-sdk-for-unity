using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk.Editor
{
    internal partial class TreasuredMapEditor
    {
        private bool _showPreview = true;

        protected override void OnSceneGUI()
        {
            HandleUtility.AddDefaultControl(0);
            if (Event.current.type == EventType.MouseDown)
            {
                _showPreview = true;
            }
            if (!_showPreview)
            {
                return;
            }
            Vector3 hotspotSize = Vector3.one * 0.3f;
            // Hotspots
            if (_showAll || _selectedObjectTab == 0)
            {
                for (int i = 0; i < _hotspots.Count; i++)
                {
                    Hotspot current = _hotspots[i];
                    if (_currentEditingObject != null && _currentEditingObject != current)
                    {
                        continue;
                    }
                    if (!current.gameObject.activeSelf)
                    {
                        continue;
                    }
                    Hotspot next = GetNextHotspot(i, _hotspots.Count);
                    switch (Tools.current)
                    {
                        //case Tool.View:
                        //    EditorGUI.BeginChangeCheck();
                        //    Vector3 newPosition = Handles.PositionHandle(Target.transform.position, Quaternion.identity);
                        //    if (EditorGUI.EndChangeCheck())
                        //    {
                        //        Undo.RecordObject(Target.transform, "Move Map Position");
                        //        Target.transform.position = newPosition;
                        //    }
                        //    break;
                        case Tool.Move:
                            EditorGUI.BeginChangeCheck();
                            Vector3 newHotspotPosition = Handles.PositionHandle(current.transform.position, current.transform.rotation);
                            if (EditorGUI.EndChangeCheck())
                            {
                                Undo.RecordObject(current.transform, "Move Hotspot Position");
                                current.transform.position = newHotspotPosition;
                            }
                            break;
                        case Tool.Rotate:
                            EditorGUI.BeginChangeCheck();
                            Quaternion newHotspotRotation = Handles.RotationHandle(current.transform.rotation, current.transform.position);
                            if (EditorGUI.EndChangeCheck())
                            {
                                Undo.RecordObject(current.transform, "Edit Hotspot Rotation");
                                current.transform.rotation = newHotspotRotation;
                            }
                            float size = HandleUtility.GetHandleSize(current.transform.position);
                            Handles.color = Color.blue;
                            Handles.ArrowHandleCap(0, current.transform.position + current.transform.forward * size, current.transform.rotation, size, EventType.Repaint);
                            break;
                    }
                    Handles.color = Color.red;
                    Handles.DrawWireCube(current.transform.position, hotspotSize);
                    Handles.color = Color.white;
                    if (current.BoxCollider)
                    {
                        Handles.DrawDottedLine(current.transform.position, current.BoxCollider.bounds.center, 5);
                    }
                    Handles.Label(current.transform.position, new GUIContent(current.gameObject.name));

                    if (_currentEditingObject == null)
                    {
                        if (next)
                        {
                            Vector3 direction = next.transform.position - current.transform.position;
                            if (direction != Vector3.zero)
                            {
                                Handles.color = Color.green;
                                Handles.ArrowHandleCap(0, current.transform.position, Quaternion.LookRotation(direction), 0.5f, EventType.Repaint);
                                //draw multiple arrows
                                //int segement = (int)(direction.magnitude / 3);
                                //for (int x = 0; x < segement; x++)
                                //{
                                //    Handles.ArrowHandleCap(0, current.transform.position + direction.normalized * x * direction.magnitude / segement, Quaternion.LookRotation(direction), 1, EventType.Repaint);
                                //}
                            }
                            Handles.color = Color.white;
                            Handles.DrawLine(current.transform.position, next.transform.position);
                        }
                    }
                }
            }
            // Interactables
            if (_showAll || _selectedObjectTab == 1)
            {
                for (int i = 0; i < _interactables.Count; i++)
                {
                    Interactable current = _interactables[i];
                    if (_currentEditingObject != null && _currentEditingObject != current)
                    {
                        continue;
                    }
                    Handles.color = Color.white;
                    Handles.Label(current.transform.position, new GUIContent(current.gameObject.name));
                    switch (Tools.current)
                    {
                        case Tool.Move:
                            EditorGUI.BeginChangeCheck();
                            Vector3 newHotspotPosition = Handles.PositionHandle(current.transform.position, current.transform.rotation);
                            if (EditorGUI.EndChangeCheck())
                            {
                                Undo.RecordObject(current.transform, "Move Interactable Position");
                                current.transform.position = newHotspotPosition;
                            }
                            break;
                        case Tool.Rotate:
                            EditorGUI.BeginChangeCheck();
                            Quaternion newHotspotRotation = Handles.RotationHandle(current.transform.rotation, current.transform.position);
                            if (EditorGUI.EndChangeCheck())
                            {
                                Undo.RecordObject(current.transform, "Edit Interactable Rotation");
                                current.transform.rotation = newHotspotRotation;
                            }
                            break;
                        case Tool.Scale:
                            if (current.BoxCollider)
                            {
                                EditorGUI.BeginChangeCheck();
                                Vector3 newSize = Handles.ScaleHandle(current.BoxCollider.size, current.transform.position + current.BoxCollider.center, current.transform.rotation, 1);
                                if (EditorGUI.EndChangeCheck())
                                {
                                    Undo.RecordObject(current.BoxCollider, "Scale Interactable Hitbox");
                                    current.BoxCollider.size = newSize;
                                }
                            }
                            break;
                    }
                }
            }
        }

        private Hotspot GetNextHotspot(int currentIndex, int totalCount)
        {
            int index = currentIndex;
            Hotspot current = _hotspots[index];
            Hotspot next = _hotspots[(index + 1) % totalCount];
            while (next != current)
            {
                if (index == totalCount - 1 && !Target.Data.Loop)
                {
                    return null;
                }
                if (next.gameObject.activeSelf)
                {
                    return next;
                }
                next = _hotspots[++index % totalCount];
            }
            return null;
        }
    }
}
