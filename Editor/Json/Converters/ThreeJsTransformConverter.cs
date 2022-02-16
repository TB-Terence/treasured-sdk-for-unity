﻿using UnityEngine;

namespace Treasured.UnitySdk
{
    internal static class ThreeJsTransformConverter
    {
        public static bool ShouldConvertToThreeJs = false;
        private static Quaternion s_threeJsRotation = Quaternion.Euler(0, 180, 0);

        public static Vector3 ToThreeJsPosition(Transform transform)
        {
            if (!ShouldConvertToThreeJs)
            {
                return transform.position;
            }
            Vector3 position = transform.position;
            position.y *= -1;
            return position;
        }

        public static Vector3 ToThreeJsRotation(Transform transform)
        {
            if (!ShouldConvertToThreeJs)
            {
                return transform.eulerAngles;
            }
            // Rotate local rotation by 180 degress and return the Euler in radians.
            return (transform.localRotation * s_threeJsRotation).eulerAngles * Mathf.Deg2Rad;
        }

        public static Vector3 ToThreeJsScale(Transform transform)
        {
            return transform.localScale;
        }
    }
}
