using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [Serializable]
    public sealed class HotspotData : TreasuredObjectData
    {

        /// <summary>
        /// Visible objects from the hotspot. Used by hotspot.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        private List<string> _visibleTargets = new List<string>();

        public List<string> VisibleTargets { get => _visibleTargets; set => _visibleTargets = value; }

        [JsonConstructor]
        private HotspotData(string id) : base(id)
        {

        }

        internal HotspotData() : base() { } // used to generate id

        public void FindVisibleTargets(IEnumerable<TreasuredObject> objects, LayerMask interactableLayer)
        {
            _visibleTargets.Clear();
            foreach (var obj in objects)
            {
                if (obj.Data.Id.Equals(this.Id))
                {
                    continue;
                }
                if (!Physics.Linecast(this.Transform.Position, obj.Data.Hitbox.Center, out RaycastHit hit, ~interactableLayer, QueryTriggerInteraction.Collide) || hit.collider == obj.BoxCollider)
                {
                    _visibleTargets.Add(obj.Data.Id);
                }
            }
        }
    }
}
