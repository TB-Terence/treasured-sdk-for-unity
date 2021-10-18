using UnityEngine;

namespace Treasured.UnitySdk
{
    /// <summary>
    /// Camera for the hotspot. Contains additional camera data for each Hotspot.
    /// </summary>
    public sealed class HotspotCamera : MonoBehaviour
    {
#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            Color tempColor = Gizmos.color;
            Matrix4x4 tempMatrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(this.transform.position, this.transform.rotation, Vector3.one);

            Gizmos.color = Color.white;
            Gizmos.DrawFrustum(Vector3.zero, 10, 0, 0.5f, 5);

            Gizmos.color = tempColor;
            Gizmos.matrix = tempMatrix;
        }
#endif
    }
}
