using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Treasured.UnitySdk
{
    /// <summary>
    /// Base class for <see cref="Hotspot"/> and <see cref="Interactable"/>.
    /// </summary>
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    public abstract class TreasuredObject : MonoBehaviour
    {
        #region Backing fields
        [SerializeField]
        [GUID]
        private string _id = Guid.NewGuid().ToString();

        [SerializeField]
        [TextArea(3, 3)]
        private string _description;

        [SerializeField]
        [Preset("FaMicrophone", "FaVolumeUp", "FaVideo", "FaLock", "FaMap", "FaTrophy", "FaHeart", "FaPlayCircle",
            "FaCat", "FaComment", "FaBoxOpen", "FaDungeon", "FaMusic", "FaPalette")]
        private string _icon;

        [SerializeField]
        private Hitbox _hitbox;

        [SerializeReference] // TODO: Remove this when upgrade is done.
        [Obsolete]
        private List<ActionBase> _onSelected = new List<ActionBase>();

        [FormerlySerializedAs("_actionGroups")]
        [SerializeReference]
        private List<ActionGroup> _onClick = new List<ActionGroup>();

        [SerializeReference]
        private List<ActionGroup> _onHover = new List<ActionGroup>();
        #endregion

        #region Properties
        /// <summary>
        /// Reference of the Map for this object.
        /// </summary>
        [JsonIgnore]
        public TreasuredMap Map => GetComponentInParent<TreasuredMap>();

        /// <summary>
        /// Global unique identifier for the object.(Read Only)
        /// </summary>
        public string Id { get => _id; }

        public string Description { get => _description; set => _description = value; }

        public string Icon { get => _icon; set => _icon = value; }
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

        [JsonIgnore]
        [Obsolete]
        public IEnumerable<ActionBase> OnSelected => _onSelected; // TODO: Remove this

        /// <summary>
        /// Group of action to perform when the object is selected.
        /// </summary>
        [JsonProperty("actionGroups")]
        public List<ActionGroup> OnClick => _onClick;

        /// <summary>
        /// Group of action to perform when the user hovers over the object.
        /// </summary>
        public List<ActionGroup> OnHover => _onHover;

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
        #endregion

#if UNITY_EDITOR
        // DO NOT REMOVE, called by Editor
        void OnSelectedInHierarchy()
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
            if (Hitbox)
            {
                var renderer = GetComponentInChildren<Renderer>();
                if (renderer && Hitbox.transform.eulerAngles == Vector3.zero)
                {
                    Hitbox.transform.eulerAngles = renderer.transform.eulerAngles;
                }
                if (!Hitbox.TryGetComponent<BoxCollider>(out var boxCollider))
                {
                    boxCollider = Hitbox.gameObject.AddComponent<BoxCollider>();
                }
                boxCollider.isTrigger = true;
            }
        }
#endif
    }
}
