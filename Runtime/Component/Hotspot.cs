using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

namespace Treasured.UnitySdk
{
    /// <summary>
    /// Interactable object used to track the transform of the camera.
    /// </summary>
    [AddComponentMenu("Treasured/Hotspot")]
    public sealed class Hotspot : TreasuredObject
    {
        #region Backing fields
        [Obsolete("Deprecated, use Hotspot.Camera to access the position instead.")]
        [SerializeField]
        private Vector3 _cameraPositionOffset = new Vector3(0, 2, 0);
        [Obsolete("Deprecated, use Hotspot.Camera to access the rotation instead.")]
        [SerializeField]
        private Vector3 _cameraRotationOffset = new Vector3();

        [SerializeField]
        private HotspotCamera _camera;

        #endregion

        #region Properties
        [JsonIgnore]
        [Obsolete("Deprecated, Use Camera.transfom.position instead. Note that the position will be in world space instead of offset.")]
        public Vector3 CameraPositionOffset { get => _cameraPositionOffset; set => _cameraPositionOffset = value; }
        [JsonIgnore]
        [Obsolete("Deprecated, Use Camera.transfom.rotation instead. Note that the rotation will be in world space instead of offset.")]
        public Vector3 CameraRotationOffset { get => _cameraRotationOffset; set => _cameraRotationOffset = value; }
        
        /// <summary>
        /// Returns camera transform for the hotspot.
        /// </summary>
        public HotspotCamera Camera
        {
            get
            {
                return _camera;
            }
            set
            {
                _camera = value;
            }
        }
        #endregion

        /// <summary>
        /// Snap the hotspot to ground if it hits collider.
        /// </summary>
        internal void SnapToGround()
        {
            // Temporarily disable self colliders
            var colliders = GetComponents<Collider>();
            Queue<bool> queue = new Queue<bool>();
            foreach (var collider in colliders)
            {
                queue.Enqueue(collider.enabled);
                collider.enabled = false;
            }
            if (Physics.Raycast(transform.position + _cameraPositionOffset, Vector3.down, out RaycastHit hit))
            {
                transform.position = hit.point + new Vector3(0, 0.01f, 0);
                if (TryGetComponent<BoxCollider>(out var collider))
                {
                    collider.center = new Vector3(0, collider.size.y / 2, 0);
                }
            }
            foreach (var collider in colliders)
            {
                collider.enabled = queue.Dequeue();
            }
        }

        /// <summary>
        /// A list of visible objects that this hotspot can see.
        /// </summary>
        public List<TreasuredObject> VisibleTargets
        {
            get
            {
                var targets = new List<TreasuredObject>();
                TreasuredMap map = GetComponentInParent<TreasuredMap>();
                if (!map || !Camera)
                {
                    return new List<TreasuredObject>();
                }
                var objects = map.GetComponentsInChildren<TreasuredObject>();
                foreach (var obj in objects)
                {
                    if (obj.Id.Equals(this.Id) || obj.Hitbox == null)
                    {
                        continue;
                    }
                    if (!Physics.Linecast(this.Camera.transform.position, obj.Hitbox.transform.position, out RaycastHit hit) || hit.collider == obj.GetComponentInChildren<Collider>()) // && hit.distance == (this.transform.transform.position - obj.Hitbox.transform.position).magnitude
                    {
                        Debug.DrawLine(this.Camera.transform.position, hit.point, Color.red, 5);
                        targets.Add(obj);
                    }
                }
                return targets;
            }
        }

        #region Editor GUI Functions
#if UNITY_EDITOR
        void OnSelectedInHierarchy()
        {
            if (_camera == null)
            {
                _camera = gameObject.FindOrCreateChild<HotspotCamera>("Camera");
                _camera.transform.localPosition = Hitbox.transform.localPosition + CameraPositionOffset;
                _camera.transform.localRotation = Quaternion.Euler(Hitbox.transform.localEulerAngles + CameraRotationOffset);
            }
        }

        void OnSceneViewFocus()
        {
            UnityEditor.SceneView.lastActiveSceneView.LookAt(Camera.transform.position, Camera.transform.rotation, 0.01f);
        }
#endif
        #endregion
    }
}
