﻿using System;
using System.Collections.Generic;
using Treasured.SDK;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [Serializable]
    public abstract class TreasuredObjectData
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
        public string Name { get; set; } // Name of the game object
        public string Description { get => _description; set => _description = value; }
        public TransformData Transform { get; set; } // Simplified transform of the game object
        public Hitbox Hitbox { get; set; } // Simplified collider of the game object
        public List<TreasuredAction> OnSelected { get => _onSelected; set => _onSelected = value; }

        protected TreasuredObjectData(string id)
        {
            this._id = id;
        }

        public void Validate()
        {
            if (string.IsNullOrEmpty(_id))
            {
                _id = Guid.NewGuid().ToString();
            }
        }
    }
}
