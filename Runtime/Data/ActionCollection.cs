using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [Serializable]
    public class ActionCollection : IList<ScriptableAction>, IEnumerable<ScriptableAction>, ICollection<ScriptableAction>, IList
    {
        public string name;
        [SerializeField]
        [HideInInspector]
        private string _id = Guid.NewGuid().ToString();
        public string Id => _id;

        public int Count => ((ICollection<ScriptableAction>)_actions).Count;

        public bool IsReadOnly => ((ICollection<ScriptableAction>)_actions).IsReadOnly;

        public bool IsFixedSize => ((IList)_actions).IsFixedSize;

        public bool IsSynchronized => ((ICollection)_actions).IsSynchronized;

        public object SyncRoot => ((ICollection)_actions).SyncRoot;

        object IList.this[int index] { get => _actions; set => _actions[index] = (ScriptableAction)value; }
        public ScriptableAction this[int index] { get => _actions[index]; set => _actions[index] = value; }

        [SerializeReference]
        private List<ScriptableAction> _actions = new List<ScriptableAction>();

        public int Add(object value)
        {
            return ((IList)_actions).Add(value);
        }

        public void Clear()
        {
            ((IList)_actions).Clear();
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

        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable)_actions).GetEnumerator();
        }

        IEnumerator<ScriptableAction> IEnumerable<ScriptableAction>.GetEnumerator()
        {
            return ((IEnumerable<ScriptableAction>)_actions).GetEnumerator();
        }

        public void Add(ScriptableAction item)
        {
            ((ICollection<ScriptableAction>)_actions).Add(item);
        }

        public bool Contains(ScriptableAction item)
        {
            return ((ICollection<ScriptableAction>)_actions).Contains(item);
        }

        public void CopyTo(ScriptableAction[] array, int arrayIndex)
        {
            ((ICollection<ScriptableAction>)_actions).CopyTo(array, arrayIndex);
        }

        public bool Remove(ScriptableAction item)
        {
            return ((ICollection<ScriptableAction>)_actions).Remove(item);
        }

        public int IndexOf(ScriptableAction item)
        {
            return ((IList<ScriptableAction>)_actions).IndexOf(item);
        }

        public void Insert(int index, ScriptableAction item)
        {
            ((IList<ScriptableAction>)_actions).Insert(index, item);
        }
    }
}
