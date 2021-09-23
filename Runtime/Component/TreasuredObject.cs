using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    public abstract class TreasuredObject : MonoBehaviour, IDataComponent<TreasuredObjectData>
    {
        [SerializeField]
        private string _id = Guid.NewGuid().ToString();

        [SerializeField]
        internal TreasuredMap _map; // Internal reference of the Map for this object, this will be set every time the object is selected.

        [JsonIgnore]
        public TreasuredMap Map
        {
            get
            {
                if (_map == null)
                {
                    _map = GetComponentInParent<TreasuredMap>();
                    if (_map == null)
                    {
                        throw new Exception("This object does not belongs to any Treasured Map. Make sure there is a Treasured Map component in parent game object.");
                    }
                }
                return _map;
            }
        }

        public string Id { get => _id; }

        /// <summary>
        /// Action to perform when the object in selected.
        /// </summary>
        [SerializeReference]
        private List<ActionBase> _onSelected = new List<ActionBase>();

        public IEnumerable<ActionBase> OnSelected => _onSelected;

        // Will be removed in next release
        [Obsolete]
        public virtual TransformData Transform
        {
            get
            {
                return new TransformData()
                {
                    Position = transform.position,
                    Rotation = transform.eulerAngles
                };
            }
        }

        // Will be removed in next release.
        [Obsolete]
        public Hitbox Hitbox
        {
            get
            {
                return new Hitbox()
                {
                    Center = _boxCollider.bounds.center, // the center on the web uses world space.
                    Size = _boxCollider.size
                };
            }
        }

        [SerializeField]
        [HideInInspector]
        [Obsolete]
        private BoxCollider _boxCollider;

        [Obsolete]
        [JsonIgnore]
        public BoxCollider BoxCollider
        {
            get
            {
                if(_boxCollider == null)
                {
                    if (TryGetComponent(out _boxCollider))
                    {
                        _boxCollider.isTrigger = true;
                    }
                }
                return _boxCollider;
            }
        }

        [JsonIgnore]
        public abstract TreasuredObjectData Data { get; }

        protected virtual void OnEnable()
        {
            if (TryGetComponent(out _boxCollider))
            {
                _boxCollider.isTrigger = true;
            }
        }

        public bool FindGroundPoint(float distance, int layerMask, out Vector3 point)
        {
            if (BoxCollider)
            {
                if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, distance, layerMask, QueryTriggerInteraction.Ignore))
                {
                    point = hit.point;
                    return true;
                }
            }
            point = Vector3.zero;
            return false;
        }

        public void OffsetHitbox(float distance = 100)
        {
            if (BoxCollider)
            {
                if (FindGroundPoint(distance, ~0, out Vector3 point))
                {
                    BoxCollider.center = point - transform.position + new Vector3(0, BoxCollider.size.y / 2, 0);       
                }
            }
        }

        void IDataComponent<TreasuredObjectData>.BindData(TreasuredObjectData data)
        {

        }
    }
}
