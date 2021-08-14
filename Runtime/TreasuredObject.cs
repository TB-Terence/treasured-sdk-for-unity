using System;
using System.Collections.Generic;
using UnityEngine;

namespace Treasured.SDK
{
    [Serializable]
    public sealed class TreasuredObject
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
        private TransformData _transform;
        /// <summary>
        /// Collider of the object.
        /// </summary>
        [SerializeField]
        private Hitbox _hitbox;
        /// <summary>
        /// List of actions that will trigger when object is selected.
        /// </summary>
        [SerializeField]
        private List<TreasuredAction> _onSelected;

        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Description { get => _description; set => _description = value; }
        public Hitbox Hitbox { get => _hitbox; set => _hitbox = value; }
        public TransformData Transform { get => _transform; set => _transform = value; }
        public List<TreasuredAction> OnSelected { get => _onSelected; set => _onSelected = value; }
    }
}
