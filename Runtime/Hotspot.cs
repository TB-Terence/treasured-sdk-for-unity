using System;
using System.Collections.Generic;
using UnityEngine;

namespace Treasured.SDK
{
    [AddComponentMenu("")]
    public sealed class Hotspot : TObject
    {
        private static readonly Vector3 HotspotSize = Vector3.one * 0.3f;

        #region JSON Properties
        /// <summary>
        /// Visible objects from the hotspot. Used by hotspot.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        private List<string> _visibleTargets = new List<string>();
        #endregion

        public List<string> VisibleTargets { get => _visibleTargets; set => _visibleTargets = value; }
    }

    public sealed class HotspotData : TObjectData
    {
        public HotspotData(Hotspot hotspot)
        {
            this._id = hotspot.Id;
            this._description = hotspot.Description;
            this._onSelected = hotspot.OnSelected;
        }
    }
}
