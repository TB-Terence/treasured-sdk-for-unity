using System;
using System.Collections.Generic;
using UnityEngine;

namespace Treasured.SDK
{
    [DisallowMultipleComponent]
    [ExecuteInEditMode]
    [AddComponentMenu("")]
    public sealed class Hotspot : MonoBehaviour
    {
        private static readonly Vector3 HotspotSize = Vector3.one * 0.3f;

        #region JSON Properties
        /// <summary>
        /// The instance id of the object.
        /// </summary>
        [SerializeField]
        [ReadOnly]
        private string _id;
        /// <summary>
        /// The display for the object.
        /// </summary>
        [SerializeField]
        private string _name;
        /// <summary>
        /// The description of the object.
        /// </summary>
        [SerializeField]
        [TextArea(3, 3)]
        private string _description;
        /// <summary>
        /// Collider of the object.
        /// </summary>
        [SerializeField]
        private Hitbox _hitbox = new Hitbox();
        /// <summary>
        /// List of actions that will trigger when object is selected.
        /// </summary>
        [SerializeField]
        private List<TreasuredAction> _onSelected = new List<TreasuredAction>();
        /// <summary>
        /// Visible objects from the hotspot. Used by hotspot.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        private List<string> _visibleTargets = new List<string>();
        #endregion

        #region Editor Properties
        private Collider _collider;

        public string Id
        {
            get
            {
                return _id;
            }
            private set
            {
                _id = value;
            }
        }
        public string Name { get => _name; set => _name = value; }
        public string Description { get => _description; set => _description = value; }
        public Hitbox Hitbox { get => _hitbox; set => _hitbox = value; }
        public List<TreasuredAction> OnSelected { get => _onSelected; set => _onSelected = value; }
        public List<string> VisibleTargets { get => _visibleTargets; set => _visibleTargets = value; }
        #endregion

        private void Awake()
        {
            if (string.IsNullOrEmpty(Id))
            {
                Id = Guid.NewGuid().ToString();
            }
        }

        private void OnEnable()
        {
            
            _collider = this.GetComponent<BoxCollider>();
            if (string.IsNullOrEmpty(Name))
            {
                Name = $"Hotspot {transform.GetSiblingIndex() + 1}";
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(transform.position, HotspotSize);
            Gizmos.color = Color.green;
            Gizmos.DrawCube(_hitbox.Center, _hitbox.Size);
        }
    }
}
