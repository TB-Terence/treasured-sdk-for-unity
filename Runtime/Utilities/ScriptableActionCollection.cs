using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Treasured.UnitySdk
{
    public class ScriptableActionCollection : ScriptableObject, IEnumerable<ScriptableAction>
    {
        [SerializeField]
        [HideInInspector]
        private string _id = Guid.NewGuid().ToString();
        public string Id => _id;
        [SerializeReference]
        private List<ScriptableAction> _actions = new List<ScriptableAction>();

        IEnumerator<ScriptableAction> IEnumerable<ScriptableAction>.GetEnumerator()
        {
            return _actions.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _actions.GetEnumerator();
        }
    }
}
