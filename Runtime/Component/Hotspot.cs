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
        private Vector3 _cameraPositionOffset = new Vector3(0, 2f, 0);

        [JsonIgnore]
        public Vector3 CameraPositionOffset { get => _cameraPositionOffset; set => _cameraPositionOffset = value; }

        [JsonIgnore]
        public TransformData CameraTransform => new TransformData()
        {
            Position = transform.position + _cameraPositionOffset,
            Rotation = transform.eulerAngles
        };

        public void SnapToGround()
        {
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit))
            {
                transform.position = hit.point + new Vector3(0, 0.01f, 0);
            }
        }

        public List<string> VisibleTargets { get; set; }

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
