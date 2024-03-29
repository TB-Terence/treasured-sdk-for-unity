﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Treasured.Actions;
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
        private string _id = Guid.NewGuid().ToString();

        [SerializeField]
        [TextArea(3, 3)]
        [JsonIgnore]
        private string _description;

        /// <summary>
        /// Name of the icon for the popup icon.
        /// </summary>
        [SerializeField]
        [Preset("FaMicrophone", "FaVolumeUp", "FaVideo", "FaLock", "FaMap", "FaTrophy", "FaHeart", "FaPlayCircle",
            "FaCat", "FaComment", "FaBoxOpen", "FaDungeon", "FaMusic", "FaPalette")]
        [OpenUrl("https://react-icons.github.io/react-icons/icons?name=fa")]
        private string _icon;

        [JsonProperty("button")]
        [FormerlySerializedAs("button")]
        public InteractableButton icon;

        [SerializeField]
        private Hitbox _hitbox;

        [FormerlySerializedAs("_actionGroups")]
        [SerializeReference]
        [Obsolete]
        // TODO: Remove after merge
        private List<ActionGroup> _onClick = new List<ActionGroup>();

        [SerializeReference]
        [Obsolete]
        // TODO: Remove after merge
        private List<ActionGroup> _onHover = new List<ActionGroup>();

        #endregion

        #region Properties
        /// <summary>
        /// Reference of the Map for this object.
        /// </summary>
        [JsonIgnore]
        public TreasuredScene Scene => GetComponentInParent<TreasuredScene>();

        /// <summary>
        /// Global unique identifier for the object.(Read Only)
        /// </summary>
        public string Id { get => _id; internal set => _id = value; }

        public string Description { get => _description; set => _description = value; }

        public Hitbox Hitbox
        {
            get
            {
                if (_hitbox == null)
                {
                    _hitbox = gameObject.FindOrCreateChild<Hitbox>("Hitbox");
                    _hitbox.transform.localPosition = Vector3.zero;
                    _hitbox.transform.localRotation = Quaternion.identity;
                    if (TryGetComponent<BoxCollider>(out var collider) && collider.isTrigger)
                    {
                        _hitbox.transform.localScale = collider.size;
                    }
                }
                return _hitbox;
            }
            set
            {
                _hitbox = value;
            }
        }

        /// <summary>
        /// Group of action to perform when the object is selected.
        /// </summary>
        // TODO: Remove after merge
        [JsonProperty("actionGroups")]
        public List<ActionGroup> OnClick => _onClick;

        /// <summary>
        /// Group of action to perform when the user hovers over the object.
        /// </summary>
        // TODO: Remove after merge
        [JsonIgnore]
        public List<ActionGroup> OnHover => _onHover;

        public ActionGraph actionGraph = new ActionGraph();

        // TODO: Remove after merge
        [JsonIgnore]
        public ScriptableActionCollection onClick;
        // TODO: Remove after merge
        [JsonIgnore]
        public ScriptableActionCollection onHover;

        #endregion
#if UNITY_EDITOR
        // DO NOT REMOVE, called by Editor
        void OnSelectedInHierarchy()
        {
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

        [ContextMenu("Copy ID")]
        void CopyID()
        {
            GUIUtility.systemCopyBuffer = Id;
        }
#endif
    }
}
