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
        private TreasuredMapData _data = new TreasuredMapData();

        [SerializeField]
        [AssetSelector(true)]
        private string _outputDirectory = "";

        public TreasuredMapData Data
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

        public string OutputDirectory { get => _outputDirectory; }

        private TreasuredMap() { }
    }
}
