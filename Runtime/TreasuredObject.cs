using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Treasured.SDK
{
    [Serializable]
    public sealed class TreasuredObject : IEquatable<TreasuredObject>
    {
        /// <summary>
        /// The display for the object.
        /// </summary>
        [SerializeField]
        private string _name;
        /// <summary>
        /// The instance id of the object.
        /// </summary>
        [SerializeField]
        private string _id;
        /// <summary>
        /// The description of the object.
        /// </summary>
        [SerializeField]
        [TextArea(3, 3)]
        private string _description;
        /// <summary>
        /// Transform of the object.
        /// </summary>
        [SerializeField]
        private TransformData _transform = new TransformData();
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

        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Description { get => _description; set => _description = value; }
        public Hitbox Hitbox { get => _hitbox; set => _hitbox = value; }
        public TransformData Transform { get => _transform; set => _transform = value; }
        public List<TreasuredAction> OnSelected { get => _onSelected; set => _onSelected = value; }
        public IEnumerable<string> VisibleTargets { get => _visibleTargets; }

        public void AddVisibleTarget(TreasuredObject target)
        {
            if (string.IsNullOrEmpty(target.Id) || target.Id.Equals(Id))
            {
                return;
            }
            if (Guid.TryParse(target.Id, out Guid result) && !_visibleTargets.Contains(target.Id))
            {
                _visibleTargets.Add(target.Id);
            }
        }

        public void ResetVisibleTargets()
        {
            _visibleTargets.Clear();
        }

        public bool Equals(TreasuredObject other)
        {
            if (other is null)
            {
                return false;
            }
            return other.Id == this.Id;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as TreasuredObject);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
