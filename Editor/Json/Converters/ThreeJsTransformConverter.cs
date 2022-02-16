using UnityEngine;
using GLTF;

namespace Treasured.UnitySdk
{
    internal static class ThreeJsTransformConverter
    {
        public static bool ShouldConvertToThreeJs = false;
        private static Quaternion s_threeJsRotation = Quaternion.Euler(0, -180, 0);

        public static Vector3 ToThreeJsPosition(Transform transform)
        {
            if (!ShouldConvertToThreeJs)
            {
                return transform.position;
            }
            Vector3 position = transform.position;
            position.x *= -1;
            return position;
        }

        public static Vector3 ToThreeJsRotation(Transform transform)
        {
            if (!ShouldConvertToThreeJs)
            {
                return transform.eulerAngles * Mathf.Deg2Rad;
            }
            return (transform.localRotation * s_threeJsRotation).eulerAngles * Mathf.Deg2Rad;
        }

        public static Vector3 ToThreeJsScale(Transform transform)
        {
            return transform.localScale;
        }
    }
}
