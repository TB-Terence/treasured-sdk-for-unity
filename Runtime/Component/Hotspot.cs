using System.Collections.Generic;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [AddComponentMenu("")]
    public sealed class Hotspot : TreasuredObject
    {
        #region JSON Properties
        /// <summary>
        /// Visible objects from the hotspot. Used by hotspot.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        private List<string> _visibleTargets = new List<string>();
        #endregion

        public List<string> VisibleTargets { get => _visibleTargets; set => _visibleTargets = value; }

        private Hotspot() { }

        protected override void OnEnable()
        {
            base.OnEnable();
        }
    }
}
