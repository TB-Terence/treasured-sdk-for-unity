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

        public TreasuredMapData Data { get => _data; }

        public string OutputDirectory { get => _outputDirectory; }
    }
}
