using System;
using System.Collections.Generic;
using Treasured.SDK;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider))]
    public abstract class TreasuredObject : MonoBehaviour
    {
        /// <summary>
        /// The instance id of the object.
        /// </summary>
        [SerializeField]
        [ReadOnly]
        protected string _id;
        /// <summary>
        /// The description of the object.
        /// </summary>
        [SerializeField]
        [TextArea(5, 5)]
        protected string _description;
        /// <summary>
        /// List of actions that will trigger when the object is selected.
        /// </summary>
        [SerializeField]
        protected List<TreasuredAction> _onSelected;

        public string Id { get => _id; }
        public string Name { get => gameObject.name; set => gameObject.name = value; }
        public string Description { get => _description; set => _description = value; }
        public List<TreasuredAction> OnSelected { get => _onSelected; set => _onSelected = value; }

        private BoxCollider _hitbox;

        public BoxCollider Hitbox { get => _hitbox; }

        protected virtual void OnEnable()
        {
            Reset();
            if (TryGetComponent(out _hitbox))
            {
                _hitbox.isTrigger = true;
            }
        }

        protected virtual void Reset()
        {
            if (string.IsNullOrEmpty(_id))
            {
                _id = Guid.NewGuid().ToString();
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

        public void OffsetHitbox()
        {
            if (Hitbox)
            {
                if (FindGroundPoint(100, ~0, out Vector3 point))
                {
                    Hitbox.center = point - transform.position + new Vector3(0, Hitbox.size.y / 2, 0);       
                }
            }
        }
    }
}
