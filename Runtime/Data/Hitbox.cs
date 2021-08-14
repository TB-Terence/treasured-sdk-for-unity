using System;
using UnityEngine;

namespace Treasured.SDK
{
    [Serializable]
    public class Hitbox
    {
        [SerializeField]
        private Vector3 _center;
        [SerializeField]
        [Vector3Range(MinX = 0.1f, MinY = 0.1f, MinZ = 0.1f)]
        private Vector3 _size;

        /// <summary>
        /// Position in world space.
        /// </summary>
        public Vector3 Center { get => _center; set => _center = value; }
        /// <summary>
        /// Boundary box of the object.
        /// </summary>
        public Vector3 Size { get => _size; set => _size = value; }
    }
}
