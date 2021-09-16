using System;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    internal partial class TreasuredMapEditorWindow
    {
        void OnSceneView(SceneView view)
        {
            if(_map == null)
            {
                return;
            }
            DrawTargetControls();
            DrawGizmos();
        }

        void DrawTargetControls()
        {
            ObjectBase target = Target as ObjectBase;
            if (target == null)
            {
                return;
            }
            // draw hitbox
            Handles.color = settings != null ? settings.hitboxColor : Color.green;
            Handles.DrawWireCube(target.hitbox.center, target.hitbox.size);
            // Controls for each type
            switch (target)
            {
                case Hotspot hotspot:
                    switch (Tools.current)
                    {
                        case Tool.Move:
                            EditorGUI.BeginChangeCheck();
                            Vector3 newCameraPosition = Handles.PositionHandle(hotspot.cameraTransform.position, hotspot.cameraTransform.Rotation);
                            if (EditorGUI.EndChangeCheck())
                            {
                                Undo.RecordObject(target, "Move hotpsot camera position");
                                hotspot.cameraTransform.position = newCameraPosition;
                            }
                            break;
                        case Tool.Rotate:
                            EditorGUI.BeginChangeCheck();
                            Quaternion newRotation = Handles.RotationHandle(hotspot.cameraTransform.Rotation, hotspot.cameraTransform.position);
                            if (EditorGUI.EndChangeCheck())
                            {
                                Undo.RecordObject(target, "Edit transform Rotation");
                                hotspot.cameraTransform.eulerAngles = newRotation.eulerAngles;
                            }
                            float size = HandleUtility.GetHandleSize(hotspot.cameraTransform.position);
                            Handles.color = Color.blue;
                            Handles.ArrowHandleCap(0, hotspot.cameraTransform.position, hotspot.cameraTransform.Rotation, size, EventType.Repaint);
                            break;
                    }
                    // draw camera cube
                    Handles.color = settings != null ? settings.cameraColor : Color.red;
                    Handles.DrawWireCube(hotspot.cameraTransform.position, Vector3.one * 0.3f);
                    // draw camera link
                    Handles.color = Color.white;
                    Handles.DrawDottedLine(hotspot.cameraTransform.position, target.hitbox.center, 5);
                    break;
                case Interactable interactable:
                    break;
            }
            // Controls in common
            switch(Tools.current)
            {
                case Tool.Move:
                    Vector3 newHitboxPosition = Handles.PositionHandle(target.hitbox.center, Quaternion.identity);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(target, "Move hitbox center");
                        target.hitbox.center = newHitboxPosition;
                    }
                    break;
                case Tool.Scale:
                    EditorGUI.BeginChangeCheck();
                    Vector3 newScale = Handles.ScaleHandle(target.hitbox.size, target.hitbox.center, Quaternion.identity, 1);
                    if (EditorGUI.EndChangeCheck())
                    {
                        newScale.x = Mathf.Clamp(newScale.x, 0.01f, newScale.x);
                        newScale.y = Mathf.Clamp(newScale.y, 0.01f, newScale.y);
                        newScale.z = Mathf.Clamp(newScale.z, 0.01f, newScale.z);
                        Undo.RecordObject(target, "Edit hitbox scale");
                        target.hitbox.size = newScale;
                    }
                    break;
            }
        }

        void DrawHotspotPath(Hotspot hotspot)
        {
            for (int i = 0; i < _map.Hotspots.Count; i++)
            {
                Hotspot current = _map.Hotspots[i];
                Hotspot next = _map.Hotspots[(i + 1) % _map.Hotspots.Count];
                if (current != hotspot)
                {
                    Handles.color = settings != null ? settings.cameraColor : Color.red;
                    Handles.DrawWireCube(current.cameraTransform.position, Vector3.one * 0.3f);
                }
                Handles.color = Color.white;
                Handles.Label(current.cameraTransform.position, current.name);

                if (!_map.loop && i == _map.Hotspots.Count - 1)
                {
                    continue;
                }
                Handles.DrawLine(current.cameraTransform.position, next.cameraTransform.position);
                Vector3 direction = next.cameraTransform.position - current.cameraTransform.position;
                if (direction != Vector3.zero)
                {
                    Handles.color = settings != null ? settings.hitboxColor : Color.green;
                    Handles.ArrowHandleCap(0, current.cameraTransform.position, Quaternion.LookRotation(direction), 0.5f, EventType.Repaint);
                }
            }
        }

        void DrawGizmos()
        {
            switch (selectedTab)
            {
                case Tabs.Map:
                    DrawHotspotsGizmo();
                    DrawInteractablesGizmo();
                    break;
                case Tabs.Hotspots:
                    if(Target == null)
                    {
                        DrawHotspotsGizmo();
                    }
                    else
                    {
                        DrawHotspotGizmo(Target as Hotspot);
                    }
                    break;
                case Tabs.Interactables:
                    if (Target == null)
                    {
                        DrawInteractablesGizmo();
                    }
                    else
                    {
                        DrawInteractableGizmo(Target as Interactable);
                    }
                    break;
            }
        }

        void DrawHotspotGizmo(Hotspot hotspot)
        {
            Handles.color = settings != null ? settings.cameraColor : Color.red;
            Handles.DrawWireCube(hotspot.cameraTransform.position, Vector3.one * 0.3f);
            Handles.color = Color.white;
            Handles.Label(hotspot.cameraTransform.position, hotspot.name);
            Handles.DrawDottedLine(hotspot.cameraTransform.position, hotspot.hitbox.center, 5);
            Handles.color = settings != null ? settings.hitboxColor : Color.green;
            Handles.DrawWireCube(hotspot.hitbox.center, hotspot.hitbox.size);
        }

        void DrawHotspotsGizmo()
        {
            for (int i = 0; i < _map.Hotspots.Count; i++)
            {
                Hotspot current = _map.Hotspots[i];
                Hotspot next = _map.Hotspots[(i + 1) % _map.Hotspots.Count];
                DrawHotspotGizmo(current);
                // Draw path
                if (!_map.loop && i == _map.Hotspots.Count - 1)
                {
                    continue;
                }
                Handles.color = settings != null ? settings.pathColor : Color.green;
                Handles.DrawLine(current.cameraTransform.position, next.cameraTransform.position);
                Vector3 direction = next.cameraTransform.position - current.cameraTransform.position;
                if (direction != Vector3.zero)
                {
                    Handles.ArrowHandleCap(0, current.cameraTransform.position, Quaternion.LookRotation(direction), 0.5f, EventType.Repaint);
                }
            }
        }

        void DrawInteractablesGizmo()
        {
            foreach (var interactable in _map.Interactables)
            {
                DrawInteractableGizmo(interactable);
            }
        }

        void DrawInteractableGizmo(Interactable interactable)
        {
            Handles.color = settings != null ? settings.hitboxColor : Color.green;
            Handles.DrawWireCube(interactable.hitbox.center, interactable.hitbox.size);
        }

    }
}
