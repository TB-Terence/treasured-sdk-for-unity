using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [Serializable]
    public abstract class ObjectBase : ScriptableObject
    {
        [SerializeField]
        private string _id = Guid.NewGuid().ToString();
        public string Id { get => _id; internal set => _id = value; }

        /// <summary>
        /// The hitbox of the object.
        /// </summary>
        public Hitbox hitbox;

        /// <summary>
        /// Action to perform when the object in selected.
        /// </summary>
        [SerializeReference]
        public List<ActionBase> onSelected = new List<ActionBase>();

        [JsonIgnore]
        public TreasuredMap Map { get; private set; }

        internal void SetMap(TreasuredMap map)
        {
            if(!EditorUtility.IsPersistent(map))
            {
                throw new ArgumentException("Map is not saved on disk.");
            }
            this.Map = map;
        }

        internal ObjectLink CreateLink()
        {
            GameObject go = new GameObject();
            ObjectLink link = go.AddComponent<ObjectLink>();
            link._map = Map;
            link._targetId = _id;
            return link;
        }
    }
}
