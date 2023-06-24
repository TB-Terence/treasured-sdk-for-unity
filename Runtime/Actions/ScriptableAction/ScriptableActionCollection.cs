using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [Serializable]
    public class ScriptableActionCollection : ScriptableObject, IEnumerable<ScriptableAction>, ICollection<ScriptableAction>, IList
    {
        [SerializeField]
        [HideInInspector]
        private string _id = Guid.NewGuid().ToString();
        public string Id => _id;

        public int Count => ((ICollection<ScriptableAction>)_actions).Count;

        public bool IsReadOnly => ((ICollection<ScriptableAction>)_actions).IsReadOnly;

        public bool IsFixedSize => ((IList)_actions).IsFixedSize;

        public bool IsSynchronized => ((ICollection)_actions).IsSynchronized;

        public object SyncRoot => ((ICollection)_actions).SyncRoot;

        public object this[int index] { get => ((IList)_actions)[index]; set => ((IList)_actions)[index] = value; }

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

        public int Add(object value)
        {
            return ((IList)_actions).Add(value);
        }

        public bool Contains(object value)
        {
            return ((IList)_actions).Contains(value);
        }

        public int IndexOf(object value)
        {
            return ((IList)_actions).IndexOf(value);
        }

        public void Insert(int index, object value)
        {
            ((IList)_actions).Insert(index, value);
        }

        public void Remove(object value)
        {
            ((IList)_actions).Remove(value);
        }

        public void RemoveAt(int index)
        {
            ((IList)_actions).RemoveAt(index);
        }

        public void CopyTo(Array array, int index)
        {
            ((ICollection)_actions).CopyTo(array, index);
        }
    }
}
