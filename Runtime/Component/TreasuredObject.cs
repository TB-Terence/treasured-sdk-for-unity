using System;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider))]
    public abstract class TreasuredObject : MonoBehaviour, IDataComponent<TreasuredObjectData>
    {
        [SerializeField]
        [HideInInspector]
        private BoxCollider _boxCollider;

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
