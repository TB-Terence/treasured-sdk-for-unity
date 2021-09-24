using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [Serializable]
    public sealed class HotspotData : TreasuredObjectData
    {

        /// <summary>
        /// Visible objects from the hotspot. Used by hotspot.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        private List<string> _visibleTargets = new List<string>();

        public List<string> VisibleTargets { get => _visibleTargets; set => _visibleTargets = value; }
    }
}
