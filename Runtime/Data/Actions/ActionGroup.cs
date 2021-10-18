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
        [SerializeReference]
        private List<ActionBase> _actions = new List<ActionBase>();

        /// <summary>
        /// List of actions to be executed in parallel.
        /// </summary>
        public List<ActionBase> Actions => _actions;
    }
}
