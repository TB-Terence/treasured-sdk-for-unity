﻿using Newtonsoft.Json;
using UnityEngine;

namespace Treasured.SDK
{
    [DisallowMultipleComponent]
    [ExecuteInEditMode]
    public sealed class TreasuredMap : MonoBehaviour
    {
        [SerializeField]
        private TreasuredMapData _data;

        [SerializeField]
        [AssetSelector(true)]
        private string _exportDirectory = "";

        private Hotspot[] _hotspots;
        private Interactable[] _interactables;

        public TreasuredMapData Data { get => _data; }

        public string ExportDirectory { get => _exportDirectory; }
        public Hotspot[] Hotspots => _hotspots;
        public Interactable[] Interactables => _interactables;

        private void OnEnable()
        {
            if (Data == null)
            {
                _data = new TreasuredMapData();
            }
            _hotspots = GetComponentsInChildren<Hotspot>(true);
            _interactables = GetComponentsInChildren<Interactable>(true);
        }
    }
}
