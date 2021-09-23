using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [AddComponentMenu("Treasured/Hotspot")]
    public sealed class Hotspot : TreasuredObject, IDataComponent<HotspotData>
    {
        [Obsolete]
        [SerializeField]
        private HotspotData _data = new HotspotData();

        [SerializeField]
        private Vector3 _cameraPositionOffset = new Vector3(0, 1.5f, 0);

        [JsonIgnore]
        public Vector3 CameraPositionOffset { get => _cameraPositionOffset; set => _cameraPositionOffset = value; }

        public override TransformData Transform
        {
            get
            {
                return new TransformData()
                {
                    Position = transform.position + _cameraPositionOffset,
                    Rotation = transform.eulerAngles
                };
            }
        }

        [JsonIgnore]
        public TransformData CameraTransform => new TransformData()
        {
            Position = transform.position + _cameraPositionOffset,
            Rotation = transform.eulerAngles
        };

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

        public void CalculateVisibleTargets()
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

        [Obsolete]
        [JsonIgnore]
        HotspotData IDataComponent<HotspotData>.Data => _data;

        public void BindData(HotspotData data)
        {
            gameObject.name = data.Name;
            gameObject.transform.position = data.Transform.Position;
            gameObject.transform.eulerAngles = data.Transform.Rotation;
            BoxCollider.center = data.Hitbox.Center - data.Transform.Position; // the Hitbox Center is in world position.
            BoxCollider.size = data.Hitbox.Size;
            _data = data;
        }

        private Hotspot() { }

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        void Reset()
        {
            _data?.Validate();
        }
    }
}
