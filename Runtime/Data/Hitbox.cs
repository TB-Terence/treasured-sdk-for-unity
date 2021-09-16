using System;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [Serializable]
    public struct Hitbox
    {
        /// <summary>
        /// The center position of the hitbox in world space.
        /// </summary>
        public Vector3 center;
        /// <summary>
        /// The size of the hitbox.
        /// </summary>
        public Vector3 size;

        public Hitbox(Vector3 center) : this(center, Vector3.one) { }

        public Hitbox(Vector3 center, Vector3 size)
        {
            this.center = center;
            this.size = size;
        }
    }
}
