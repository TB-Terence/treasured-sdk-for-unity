using System;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [Serializable]
    public sealed class Hitbox
    {
        [SerializeField]
        private Vector3 _center;
        [SerializeField]
        private Vector3 _size;

        /// <summary>
        /// Center point of the hitbox in world space.
        /// </summary>
        public Vector3 Center { get => _center; set => _center = value; }
        /// <summary>
        /// Boundary box of the object.
        /// </summary>
        public Vector3 Size { get => _size; set => _size = value; }

        public static implicit operator Hitbox(BoxCollider collider)
        {
            return new Hitbox()
            {
                _center = collider.bounds.center, // the center on the web uses world space.
                _size = collider.size
            };
        }
    }
}
