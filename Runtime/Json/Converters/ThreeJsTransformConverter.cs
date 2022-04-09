using UnityEngine;
using UnityGLTF.Extensions;

namespace Treasured.UnitySdk
{
    internal static class ThreeJsTransformConverter
    {
        public static bool ShouldConvertToThreeJsTransform = true;
        public static Vector3 ToThreeJsPosition(Transform transform)
        {
            if (!ShouldConvertToThreeJsTransform)
            {
                return transform.position;
            }
            GLTF.Math.Vector3 position = transform.position.ToGltfVector3Convert();
            return new Vector3(position.X, position.Y, position.Z);
        }

        public static Quaternion ToThreeJsQuaternion(Transform transform)
        {
            if (!ShouldConvertToThreeJsTransform)
            {
                return transform.rotation;
            }
            GLTF.Math.Quaternion quaternion = transform.rotation.ToGltfQuaternionConvert();
            return new Quaternion(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);
        }
        public static Vector3 ToThreeJsEulerAngles(Transform transform)
        {
            if (!ShouldConvertToThreeJsTransform)
            {
                return transform.eulerAngles;
            }
            GLTF.Math.Quaternion quaternion = transform.rotation.ToGltfQuaternionConvert();
            return new Quaternion(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W).eulerAngles;
        }

        public static Vector3 ToThreeJsScale(Transform transform)
        {
            if (!ShouldConvertToThreeJsTransform)
            {
                return transform.localScale;
            }
            GLTF.Math.Vector3 scale = transform.localScale.ToGltfVector3Convert();
            return new Vector3(scale.X, scale.Y, scale.Z);
        }
    }
}
