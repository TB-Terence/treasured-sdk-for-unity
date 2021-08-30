using UnityEngine;

namespace Treasured.UnitySdk
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider))]
    public abstract class TreasuredObject : MonoBehaviour//, IDataComponent<TreasuredObjectData>
    {
        [SerializeField]
        [HideInInspector]
        private BoxCollider _hitbox;

        public BoxCollider Hitbox
        {
            get
            {
                if(_hitbox == null)
                {
                    if (TryGetComponent(out _hitbox))
                    {
                        _hitbox.isTrigger = true;
                    }
                }
                return _hitbox;
            }
        }

        //public abstract TreasuredObjectData Data { get; }
        //protected abstract TreasuredObjectData GetData();

        protected virtual void OnEnable()
        {
            if (TryGetComponent(out _hitbox))
            {
                _hitbox.isTrigger = true;
            }
        }

        public bool FindGroundPoint(float distance, int layerMask, out Vector3 point)
        {
            if (Hitbox)
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
            if (Hitbox)
            {
                if (FindGroundPoint(distance, ~0, out Vector3 point))
                {
                    Hitbox.center = point - transform.position + new Vector3(0, Hitbox.size.y / 2, 0);       
                }
            }
        }
    }
}
