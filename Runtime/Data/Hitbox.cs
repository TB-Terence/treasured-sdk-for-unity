using System;
using Treasured.SDK;
using UnityEngine;

namespace Treasured.UnitySdk
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

        public static implicit operator Hitbox(Collider collider)
        {
            return new Hitbox()
            {
                _center = collider.bounds.center,
                _size = collider.bounds.extents
            };
        }
    }
}
