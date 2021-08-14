using UnityEngine;

namespace Treasured.SDK
{
    public class Vector3RangeAttribute : PropertyAttribute
    {
        private float _minX = -100000;
        private float _minY = -100000;
        private float _minZ = -100000;
        private float _maxX = 100000;
        private float _maxY = 100000;
        private float _maxZ = 100000;

        /// <summary>
        /// Default is -100000
        /// </summary>
        public float MinX { get => _minX; set => _minX = value; }
        /// <summary>
        /// Default is -100000
        /// </summary>
        public float MinY { get => _minY; set => _minY = value; }
        /// <summary>
        /// Default is -100000
        /// </summary>
        public float MinZ { get => _minZ; set => _minZ = value; }

        /// <summary>
        /// Default is 100000
        /// </summary>
        public float MaxX { get => _maxX; set => _maxX = value; }

        /// <summary>
        /// Default is 100000
        /// </summary>
        public float MaxY { get => _maxY; set => _maxY = value; }

        /// <summary>
        /// Default is 100000
        /// </summary>
        public float MaxZ { get => _maxZ; set => _maxZ = value; }
    }
}
