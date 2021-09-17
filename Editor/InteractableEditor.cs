using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk.Editor
{
    [CustomEditor(typeof(Interactable))]
    internal class InteractableEditor : TreasuredObjectEditor<Interactable, InteractableData>
    {
        protected override void Init()
        {
            base.Init();
            Tools.hidden = true;
        }

        private void OnDisable()
        {
            Tools.hidden = false; // show the transform tools for other game object
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }

        protected override void OnSceneGUI()
        {
            base.OnSceneGUI();
            switch (Tools.current)
            {
                case Tool.Move:
                    EditorGUI.BeginChangeCheck();
                    Vector3 newHotspotPosition = Handles.PositionHandle(Target.transform.position, Target.transform.rotation);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(Target.transform, "Move Interactable Position");
                        Target.transform.position = newHotspotPosition;
                    }
                    break;
                case Tool.Rotate:
                    EditorGUI.BeginChangeCheck();
                    Quaternion newHotspotRotation = Handles.RotationHandle(Target.transform.rotation, Target.transform.position);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(Target.transform, "Edit Interactable Rotation");
                        Target.transform.rotation = newHotspotRotation;
                    }
                    break;
                case Tool.Scale:
                    if (Target.BoxCollider)
                    {
                        EditorGUI.BeginChangeCheck();
                        Vector3 newSize = Handles.ScaleHandle(Target.BoxCollider.size, Target.BoxCollider.bounds.center, Target.transform.rotation, 1);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(Target.BoxCollider, "Scale Interactable Hitbox");
                            Target.BoxCollider.size = newSize;
                        }
                    }
                    break;
            }
        }
    }
}
