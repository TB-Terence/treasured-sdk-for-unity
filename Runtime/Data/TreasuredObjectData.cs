using Newtonsoft.Json;
using System;
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

        private Hitbox _hitbox;

        public string Id { get => _id; }
        public string Description { get => _description; set => _description = value; }
        public TransformData Transform { get; set; }
        public Hitbox Hitbox { get; set; }
        public List<TreasuredAction> OnSelected { get => _onSelected; set => _onSelected = value; }

        protected TreasuredObjectData(TreasuredObject obj)
        {
            if (obj == null)
            {
                return;
            }
            this._id = obj.Id;
            this.Description = obj.Description;
            this.Transform = obj.transform;
            this.Hitbox = obj.Hitbox;
            this.OnSelected = obj.OnSelected;
        }

        protected TreasuredObjectData(string id)
        {
            this._id = id;
        }
    }
}
