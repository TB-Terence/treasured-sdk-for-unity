using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    public abstract class TreasuredObject : MonoBehaviour
    {
        [SerializeField]
        [GUID]
        private string _id = Guid.NewGuid().ToString();

        internal TreasuredMap _map; // Internal reference of the Map for this object, this will be set every time the object is selected.

        [JsonIgnore]
        public TreasuredMap Map
        {
            get
            {
                if (_map == null)
                {
                    _map = GetComponentInParent<TreasuredMap>();
                }
                return _map;
            }
        }

        public string Id { get => _id; }

        [SerializeField]
        [TextArea(3, 3)]
        private string _description;

        public string Description { get => _description; set => _description = value; }

        public virtual TransformData Transform
        {
            get
            {
                return new TransformData()
                {
                    Position = transform.position,
                    Rotation = transform.eulerAngles
                };
            }
        }

        public Hitbox Hitbox
        {
            get
            {
                var boxCollider = GetComponent<BoxCollider>();
                return new Hitbox()
                {
                    Center = boxCollider ? boxCollider.bounds.center : transform.position, // the center on the web uses world space.
                    Size = boxCollider ? boxCollider.size : Vector3.one
                };
            }
        }

        /// <summary>
        /// Action to perform when the object in selected.
        /// </summary>
        [SerializeReference]
        private List<ActionBase> _onSelected = new List<ActionBase>();

        [SerializeReference]
        private List<ActionGroup> _actionGroups = new List<ActionGroup>();

        public IEnumerable<ActionBase> OnSelected => _onSelected;
        public List<ActionGroup> ActionGroups => _actionGroups;

        [JsonIgnore]
        [Obsolete]
        public abstract TreasuredObjectData Data { get; }

        //public Color ObjectId
        //{
        //    get
        //    {
        //        int seed = Id.GetHashCode();
        //        System.Random rand = new System.Random(seed);
        //        byte[] buffer = new byte[3];
        //        rand.NextBytes(buffer);
        //        return new Color32(buffer[0], buffer[1], buffer[2], 255); // ColorUtility.ToHtmlStringRGB internally uses Color32 and use Color causes some precision error in the final output
        //    }
        //}
    }
}
