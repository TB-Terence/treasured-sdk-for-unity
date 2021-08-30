using System.Collections.Generic;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [AddComponentMenu("")]
    public sealed class Hotspot : TreasuredObject
    {
        [SerializeField]
        private HotspotData _data;
        public HotspotData Data
        {
            get
            {
                return _data;
            }
            set
            {
                _data = value;
            }
        }

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

        void Reset()
        {
            _data?.Validate();
        }

        public override void LoadFromData(TreasuredObjectData data)
        {
            base.LoadFromData(data);
            _data = (HotspotData)data;
        }
    }
}
