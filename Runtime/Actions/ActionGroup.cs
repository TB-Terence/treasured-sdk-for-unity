using System;
using System.Collections.Generic;
using UnityEngine;

namespace Treasured.UnitySdk
{
    /// <summary>
    /// Defines a group of actions that can be executed in parallel.
    /// </summary>
    [Serializable]
    public class ActionGroup : ScriptableObject
    {
        [SerializeField]
        private string _id = Guid.NewGuid().ToString();
        public string Id { get { return _id; } }
        [SerializeReference]
        private List<Action> _actions = new List<Action>();

        /// <summary>
        /// List of actions to be executed in parallel.
        /// </summary>
        public List<Action> Actions => _actions;
    }
}
