using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [AddComponentMenu("Treasured/Hotspot")]
    public sealed class Hotspot : TreasuredObject
    {
        [Obsolete]
        [SerializeField]
        private HotspotData _data = new HotspotData();

        [SerializeField]
        private Vector3 _cameraPositionOffset = new Vector3(0, 2, 0);
        [SerializeField]
        private Vector3 _cameraRotationOffset = new Vector3();

        [SerializeField]
        private Transform _hitboxTransform;
        [SerializeField]
        private Transform _cameraTransform;

        [JsonIgnore]
        public Vector3 CameraPositionOffset { get => _cameraPositionOffset; set => _cameraPositionOffset = value; }
        [JsonIgnore]
        public Vector3 CameraRotationOffset { get => _cameraRotationOffset; set => _cameraRotationOffset = value; }

        public override Transform Transform
        {
            get => _hitboxTransform;
        }

        /// <summary>
        /// Returns camera transform for the hotspot.
        /// </summary>
        public Transform CameraTransform
        {
            get
            {
                return _cameraTransform;
            }
            set
            {
                _cameraTransform = value;
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

        [JsonIgnore]
        [Obsolete]
        public override TreasuredObjectData Data
        {
            get
            {
                return _data;
            }
        }

#if UNITY_EDITOR
        internal void GroupHotspot()
        {
            if (_hitboxTransform == null)
            {
                _hitboxTransform = gameObject.FindOrCreateChild("Hitbox");
                
                _hitboxTransform.localPosition = this.transform.localPosition;
                _hitboxTransform.localRotation = this.transform.localRotation;
                this.transform.position = Vector3.zero;
                this.transform.rotation = Quaternion.identity;
            }

            if (_cameraTransform == null)
            {
                _cameraTransform = gameObject.FindOrCreateChild("Camera");
                _cameraTransform.localPosition = _hitboxTransform.localPosition + CameraPositionOffset;
                _cameraTransform.localRotation = Quaternion.Euler(_hitboxTransform.localEulerAngles + CameraRotationOffset);
            }

            UnityEditor.EditorUtility.SetDirty(gameObject);
        }
#endif
    }
}
