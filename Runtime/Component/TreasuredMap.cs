using System;
using Treasured.SDK;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Treasured/Treasured Map")]
    public sealed class TreasuredMap : MonoBehaviour
    {
        [SerializeField]
        private TreasuredMapData _data;

        [SerializeField]
        [AssetSelector(true)]
        private string _outputDirectory = "";

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
