using UnityEngine;

namespace Treasured.UnitySdk
{
    public sealed class Hitbox : MonoBehaviour
    {
#if UNITY_EDITOR
        private static Color boxColor = new Color(0, 1, 0, 0.2f);

        void OnDrawGizmosSelected()
        {
            Color tempColor = Gizmos.color;
            Matrix4x4 tempMatrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(this.transform.position, this.transform.rotation, Vector3.one);

            Gizmos.color = boxColor;
            Gizmos.DrawCube(Vector3.zero, transform.localScale);

            Gizmos.color = tempColor;
            Gizmos.matrix = tempMatrix;
        }
#endif
    }
}
