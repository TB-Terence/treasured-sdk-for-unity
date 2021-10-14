using System;
using System.Collections.Generic;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [Serializable]
    public class ActionGroup : ScriptableObject
    {
        [SerializeReference]
        private List<ActionBase> _actions = new List<ActionBase>();

        public IEnumerable<ActionBase> Actions => _actions;
    }
}
