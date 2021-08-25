using System;
using System.Collections.Generic;
using Treasured.SDK;
using UnityEngine;

namespace Treasured
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    public abstract class TObject : MonoBehaviour
    {
        /// <summary>
        /// The instance id of the object.
        /// </summary>
        [SerializeField]
        [ReadOnly]
        protected string _id;
        /// <summary>
        /// The display for the object.
        /// </summary>
        [SerializeField]
        protected string _name;
        /// <summary>
        /// The description of the object.
        /// </summary>
        [SerializeField]
        [TextArea(3, 3)]
        protected string _description;
        /// <summary>
        /// List of actions that will trigger when the object is selected.
        /// </summary>
        [SerializeField]
        protected List<TreasuredAction> _onSelected;

        public string Id { get => _id; }
        public string Name { get => _name; set => _name = value; }
        public string Description { get => _description; set => _description = value; }
        public List<TreasuredAction> OnSelected { get => _onSelected; set => _onSelected = value; }

        private void Awake()
        {
            if (string.IsNullOrEmpty(_id))
            {
                _id = Guid.NewGuid().ToString();
            }
        }
    }
}
