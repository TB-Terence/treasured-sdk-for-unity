using UnityEngine;
using UnityGLTF.Extensions;

namespace Treasured.UnitySdk
{
    internal static class ThreeJsTransformConverter
    {
        public static GLTF.Math.Vector3 ToThreeJsPosition(Transform transform)
        {
            return transform.position.ToGltfVector3Convert();
        }

        public static GLTF.Math.Quaternion ToThreeJsRotation(Transform transform)
        {
            return transform.rotation.ToGltfQuaternionConvert();
        }

        public static GLTF.Math.Vector3 ToThreeJsScale(Transform transform)
        {
            return transform.localScale.ToGltfVector3Convert();
        }
    }
}
