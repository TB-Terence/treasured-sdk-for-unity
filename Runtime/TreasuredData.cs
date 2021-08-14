﻿using System.Collections.Generic;
using UnityEngine;

namespace Treasured.SDK
{
    public enum ImageFormat
    {
        JPEG,
        PNG
    }
    public enum ImageQuality
    {
        Low = 1024,
        Medium = 2048,
        High = 4096,
        Ultra = 8192
    }

    [CreateAssetMenu(fileName = "Data", menuName = "Treasured/Data")]
    public sealed class TreasuredData : ScriptableObject
    {
        public static readonly string Version = "0.3.0";

        [SerializeField]
        private string _name;
        [SerializeField]
        private bool _loop;
        [SerializeField]
        private ImageFormat _format = ImageFormat.PNG;
        [SerializeField]
        private ImageQuality _quality = ImageQuality.Medium;
        [SerializeField]
        private List<TreasuredObject> _hotspots;
        [SerializeField]
        private List<TreasuredObject> _interactables;

        public string Name { get => _name; set => _name = value; }
        public bool Loop { get => _loop; set => _loop = value; }
        public ImageFormat Format { get => _format; set => _format = value; }
        public ImageQuality Quality { get => _quality; set => _quality = value; }
        public List<TreasuredObject> Hotspots { get => _hotspots; set => _hotspots = value; }
        public List<TreasuredObject> Interactables { get => _interactables; set => _interactables = value; }
    }
}
