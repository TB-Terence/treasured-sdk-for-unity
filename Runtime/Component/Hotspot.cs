using System.Collections.Generic;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [AddComponentMenu("")]
    public sealed class Hotspot : TreasuredObject, IDataComponent<HotspotData>
    {
        [SerializeField]
        private HotspotData _data;

        public HotspotData Data
        {
            get
            {
                return _data;
            }
        }

        public void BindData(HotspotData data)
        {
            gameObject.name = data.Name;
            gameObject.transform.position = data.Transform.Position;
            gameObject.transform.eulerAngles = data.Transform.Rotation;
            Hitbox.center = data.Hitbox.Center;
            Hitbox.size = data.Hitbox.Size;
            _data = data;
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
    }
}
