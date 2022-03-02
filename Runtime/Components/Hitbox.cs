using UnityEngine;

namespace Treasured.UnitySdk
{
    [RequireComponent(typeof(BoxCollider))]
    public sealed class Hitbox : MonoBehaviour
    {
#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            Color tempColor = Gizmos.color;
            Matrix4x4 tempMatrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(this.transform.position, this.transform.rotation, Vector3.one);

            if (transform.parent != null && transform.parent.GetComponent<Hotspot>())
            {
                Gizmos.color = Color.red;
            }
            else
            {
                //Gizmos.color = TreasuredSDKSettings.Instance ? TreasuredSDKSettings.Instance.hitboxColor : TreasuredSDKSettings.defaultHitboxColor;
            }

            Gizmos.DrawCube(Vector3.zero, transform.localScale);

            Gizmos.color = tempColor;
            Gizmos.matrix = tempMatrix;
        }
#endif
    }
}
