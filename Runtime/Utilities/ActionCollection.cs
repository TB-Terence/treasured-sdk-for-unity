﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Treasured.UnitySdk
{
    public class ActionCollection : ScriptableObject, IEnumerable<Action>
    {
        [SerializeField]
        [HideInInspector]
        private string _id = Guid.NewGuid().ToString();
        public string Id => _id;
        [SerializeReference]
        private List<Action> _actions = new List<Action>();

        IEnumerator<Action> IEnumerable<Action>.GetEnumerator()
        {
            return _actions.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _actions.GetEnumerator();
        }
    }
}
