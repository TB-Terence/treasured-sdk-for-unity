using System;
using System.Collections.Generic;
using System.Linq;
using Treasured.UnitySdk;
using UnityEngine;

namespace Treasured.Actions
{
    [Serializable]
    public class ActionGraph
    {
        [SerializeReference]
        private List<ScriptableActionCollection> _groups = new List<ScriptableActionCollection>();
        
        public ScriptableActionCollection AddActionGroup(string groupName)
        {
            if (!TryGetActionGroup(groupName, out var group))
            {
                group = ScriptableObject.CreateInstance<ScriptableActionCollection>();
                group.name = groupName;
                _groups.Add(group);
            }
            return group;
        }

        public bool TryGetActionGroup(string groupName, out ScriptableActionCollection group)
        {
            group = _groups.FirstOrDefault(group => group != null && group.name == groupName);
            return group != null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns>Returns true if group is removed. </returns>
        public bool RemoveActionGroup(string groupName)
        {
            return TryGetActionGroup(groupName, out var group) && _groups.Remove(group);
        }

        public IEnumerable<ScriptableActionCollection> GetGroups()
        {
            return _groups;
        }
    }
}
