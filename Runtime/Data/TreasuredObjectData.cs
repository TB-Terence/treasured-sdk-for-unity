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

        public string Id { get => _id; }
        public string Description { get => _description; set => _description = value; }
        public List<TreasuredAction> OnSelected { get => _onSelected; set => _onSelected = value; }
    }
}
