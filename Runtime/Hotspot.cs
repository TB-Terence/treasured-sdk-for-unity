using System;
using System.Collections.Generic;
using UnityEngine;

namespace Treasured.SDK
{
    [AddComponentMenu("")]
    public sealed class Hotspot : TObject
    {
        private static readonly Vector3 HotspotSize = Vector3.one * 0.3f;

        #region JSON Properties
        /// <summary>
        /// Collider of the object.
        /// </summary>
        [SerializeField]
        private Hitbox _hitbox = new Hitbox();
        /// <summary>
        /// Visible objects from the hotspot. Used by hotspot.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        private List<string> _visibleTargets = new List<string>();
        #endregion

        #region Editor Properties
        private Collider _collider;
        #endregion

        public Hitbox Hitbox { get => _hitbox; set => _hitbox = value; }
        public List<string> VisibleTargets { get => _visibleTargets; set => _visibleTargets = value; }

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
