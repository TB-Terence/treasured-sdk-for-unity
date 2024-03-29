﻿using System.Collections.Generic;
using UnityEngine;

namespace Treasured.UnitySdk
{
    /// <summary>
    /// Interactable object used to track the transform of the camera.
    /// </summary>
    [AddComponentMenu("Treasured/Hotspot")]
    public sealed class Hotspot : TreasuredObject
    {
        #region Backing fields

        [SerializeField]
        private HotspotCamera _camera;

        #endregion

        #region Properties
        /// <summary>
        /// Returns camera transform for the hotspot.
        /// </summary>
        public HotspotCamera Camera
        {
            get
            {
                return _camera;
            }
            set
            {
                _camera = value;
            }
        }
        #endregion

        private void OnEnable()
        {
            // add default action group for onSelect event
            actionGraph.AddActionGroup("onSelect");
            if (_camera == null)
            {
                _camera = gameObject.FindOrCreateChild<HotspotCamera>("Camera");
                _camera.transform.localPosition = Hitbox.transform.localPosition;
                _camera.transform.localRotation = Quaternion.Euler(Hitbox.transform.localEulerAngles);
            }
        }

        /// <summary>
        /// A list of visible objects that this hotspot can see.
        /// </summary>
        //[JsonIgnore]
        public List<TreasuredObject> VisibleTargets
        {
            get
            {
                var targets = new List<TreasuredObject>();
                TreasuredScene scene = GetComponentInParent<TreasuredScene>();
                if (!scene || !Camera)
                {
                    return new List<TreasuredObject>();
                }
                var objects = scene.GetComponentsInChildren<TreasuredObject>();
                foreach (var obj in objects)
                {
                    if (obj.Id.Equals(this.Id) || obj.Hitbox == null)
                    {
                        continue;
                    }
                    if (!Physics.Linecast(this.Camera.transform.position, obj.Hitbox.transform.position, out RaycastHit hit) || hit.collider == obj.GetComponentInChildren<Collider>()) // && hit.distance == (this.transform.transform.position - obj.Hitbox.transform.position).magnitude
                    {
                        targets.Add(obj);
                    }
                }
                return targets;
            }
        }

        private void OnDrawGizmos()
        {
            if (Hitbox)
            {
                Gizmos.DrawIcon(Hitbox.transform.position, "Packages/com.treasured.unitysdk/Resources/Hotspot.png", true);
            }
        }

        #region Editor GUI Functions
#if UNITY_EDITOR
        void OnSelectedInHierarchy()
        {
            if (_camera == null)
            {
                _camera = gameObject.FindOrCreateChild<HotspotCamera>("Camera");
                _camera.transform.localPosition = Hitbox.transform.localPosition;
                _camera.transform.localRotation = Quaternion.Euler(Hitbox.transform.localEulerAngles);
            }
        }
#endif
        #endregion
    }
}
