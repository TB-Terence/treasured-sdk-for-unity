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
            var group = GetActionGroup(groupName);
            if (group != null)
            {
                return group;
            }
            group = new ScriptableActionCollection
            {
                name = groupName
            };
            _groups.Add(group);
            return group;
        }

        public ScriptableActionCollection GetActionGroup(string groupName)
        {
            return _groups.FirstOrDefault(group => group.name == groupName);
        }

        public bool RemoveActionGroup(string groupName)
        {
            var group = GetActionGroup(groupName);
            if (group != null)
            {
               return _groups.Remove(group);
            }
            return false;
        }

        public IEnumerable<ScriptableActionCollection> GetGroups()
        {
            return _groups;
        }
    }
}
