using UnityEngine;

namespace Treasured.UnitySdk
{
    internal static class ThreeJsTransformConverter
    {
        public static bool ShouldConvertToThreeJs = false;

        public static Vector3 ToThreeJsPosition(Vector3 position)
        {
            if (!ShouldConvertToThreeJs)
            {
                return position;
            }
           // position.x = -position.x;
            return position;
        }

        public static Quaternion ToThreeJsRotation(Quaternion rotation)
        {
            if (!ShouldConvertToThreeJs)
            {
                return rotation ;
            }
            return rotation;
        }

        public static Vector3 ToThreeJsScale(Vector3 scale)
        {
            if (!ShouldConvertToThreeJs)
            {
                return scale;
            }
        //    scale.x = -scale.x;
       //     scale.z = -scale.z;
            return scale;
        }
    }
}
