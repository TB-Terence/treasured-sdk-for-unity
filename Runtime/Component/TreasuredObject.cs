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

        [SerializeField]
        private Hitbox _hitbox;

        public Hitbox Hitbox
        {
            get
            {
                return _hitbox;
            }
            set
            {
                _hitbox = value;
            }
        }

        [SerializeReference]
        [Obsolete]
        private List<ActionBase> _onSelected = new List<ActionBase>();

        /// <summary>
        /// Group of action to perform when the object is selected.
        /// </summary>
        [SerializeReference]
        private List<ActionGroup> _actionGroups = new List<ActionGroup>();

        [JsonIgnore]
        [Obsolete]
        public IEnumerable<ActionBase> OnSelected => _onSelected; // TODO: Remove this
        public List<ActionGroup> ActionGroups => _actionGroups;

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

#if UNITY_EDITOR
        internal void CreateTransformGroupInternal()
        {
            CreateTransformGroup();
            UnityEditor.EditorUtility.SetDirty(gameObject);
        }
#endif

        protected virtual void CreateTransformGroup()
        {
            if (Hitbox == null)
            {
                Hitbox = gameObject.FindOrCreateChild<Hitbox>("Hitbox");
                Hitbox.transform.localPosition = Vector3.zero;
                Hitbox.transform.localRotation = Quaternion.identity;
                if (TryGetComponent<BoxCollider>(out var collider) && collider.isTrigger)
                {
                    Hitbox.transform.localScale = collider.size;
                }
            }
        }
    }
}
