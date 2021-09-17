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
        #endregion

        #region Map Data
        [SerializeField]
        private TreasuredMapData _data = new TreasuredMapData();
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
