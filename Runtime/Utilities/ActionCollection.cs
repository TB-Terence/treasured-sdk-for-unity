using System;
using System.Collections.Generic;
using UnityEngine;

namespace Treasured.UnitySdk
{
    public class ActionCollection : ScriptableObject
    {
        [SerializeField]
        [HideInInspector]
        private string _id = Guid.NewGuid().ToString();
        public string Id => _id;
        [SerializeReference]
        public List<Action> actions = new List<Action>();
    }
}
