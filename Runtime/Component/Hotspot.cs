using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [AddComponentMenu("Treasured/Hotspot")]
    public sealed class Hotspot : TreasuredObject
    {
        [SerializeField]
        private Vector3 _cameraPositionOffset = new Vector3(0, 2, 0);
        [SerializeField]
        private Vector3 _cameraRotationOffset = new Vector3();

        [SerializeField]
        private HotspotCamera _camera;

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

        public void SnapToGround()
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

        public List<string> VisibleTargets
        {
            get; private set;
        }

        public void ComputeVisibleTargets()
        {
            VisibleTargets = new List<string>();
            TreasuredMap map = GetComponentInParent<TreasuredMap>();
            if (!map)
            {
                return;
            }
            var objects = map.GetComponentsInChildren<TreasuredObject>();
            foreach (var obj in objects)
            {
                if (obj.Id.Equals(this.Id))
                {
                    continue;
                }
                if (!Physics.Linecast(this.transform.position + _cameraPositionOffset, obj.transform.position, out RaycastHit hit) || hit.collider == obj.GetComponent<Collider>())
                {
                    VisibleTargets.Add(obj.Id);
                }
            }
        }

        protected override void CreateTransformGroup()
        {
            base.CreateTransformGroup();
            if (_camera == null)
            {
                _camera = gameObject.FindOrCreateChild<HotspotCamera>("Camera");
                _camera.transform.localPosition = Hitbox.transform.localPosition + CameraPositionOffset;
                _camera.transform.localRotation = Quaternion.Euler(Hitbox.transform.localEulerAngles + CameraRotationOffset);
            }
        }
    }
}
