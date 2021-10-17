using UnityEngine;

namespace Treasured.UnitySdk
{
    public sealed class Hitbox : MonoBehaviour
    {
#if UNITY_EDITOR
        private static Color boxColor = new Color(0, 1, 0, 0.2f);

        void OnDrawGizmosSelected()
        {
            Gizmos.color = boxColor;
            Gizmos.DrawCube(transform.position, transform.localScale);
        }
#endif
    }
}
