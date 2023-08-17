using System.Collections.Generic;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [RequireComponent(typeof(BoxCollider))]
    public sealed class Hitbox : MonoBehaviour
    {
        /// <summary>
        /// Snap self to ground if hits collider.
        /// </summary>
        public void SnapToGround()
        {
            // Temporarily disable self colliders
            var colliders = GetComponents<Collider>();
            Queue<bool> queue = new Queue<bool>();
            foreach (var collider in colliders)
            {
                queue.Enqueue(collider.enabled);
                collider.enabled = false;
            }
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit))
            {
                transform.position = hit.point + new Vector3(0, 0.01f, 0);
                var collider = GetComponentInChildren<BoxCollider>();
                if (collider)
                {
                    collider.center = new Vector3(0, collider.size.y / 2, 0);
                }
            }
            foreach (var collider in colliders)
            {
                collider.enabled = queue.Dequeue();
            }
        }
    }
}
