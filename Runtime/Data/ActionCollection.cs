using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [Serializable]
    public class ActionCollection : IEnumerable<ScriptableAction>, ICollection<ScriptableAction>
    {
        public string name;
        [SerializeField]
        [HideInInspector]
        private string _id = Guid.NewGuid().ToString();
        public string Id => _id;

        public int Count => ((ICollection<ScriptableAction>)_actions).Count;

        public bool IsReadOnly => ((ICollection<ScriptableAction>)_actions).IsReadOnly;

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

        public void Add(ScriptableAction item)
        {
            ((ICollection<ScriptableAction>)_actions).Add(item);
        }

        public void Clear()
        {
            ((ICollection<ScriptableAction>)_actions).Clear();
        }

        public bool Contains(ScriptableAction item)
        {
            return ((ICollection<ScriptableAction>)_actions).Contains(item);
        }

        public bool Contains(string id)
        {
            return _actions.Any(x => x.Id.Equals(id));
        }

        public void CopyTo(ScriptableAction[] array, int arrayIndex)
        {
            ((ICollection<ScriptableAction>)_actions).CopyTo(array, arrayIndex);
        }

        public bool Remove(ScriptableAction item)
        {
            return ((ICollection<ScriptableAction>)_actions).Remove(item);
        }
    }
}
