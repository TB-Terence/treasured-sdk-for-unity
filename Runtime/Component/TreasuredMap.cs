using UnityEngine;

namespace Treasured.UnitySdk
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Treasured/Treasured Map")]
    public sealed class TreasuredMap : MonoBehaviour
    {
        #region Map Settings
        [SerializeField]
        private LayerMask _interactableLayer;
        [SerializeField]
        [Range(-5, 15)]
#pragma warning disable IDE0051 // Remove unused private members
        private float _fixedExposure = 13;
#pragma warning restore IDE0051 // Remove unused private members
        #endregion

        #region Map Data
        [SerializeField]
        private TreasuredMapData _data;
        #endregion

        #region Export Settings
        [SerializeField]
        [AssetSelector(true)]
        private string _outputDirectory = "";
        #endregion

        public TreasuredMapData Data
        {
            get
            {
                if (_data == null)
                {
                    _data = new TreasuredMapData();
                }
                return _data;
            }
            set
            {
                _data = value;
            }
        }

        public string OutputDirectory { get => _outputDirectory; }

        private TreasuredMap() { }
    }
}
