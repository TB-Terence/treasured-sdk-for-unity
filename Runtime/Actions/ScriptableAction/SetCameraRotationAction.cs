using UnityEngine;

namespace Treasured.UnitySdk
{
    [API("setCameraRotation")]
    public class SetCameraRotationAction : ScriptableAction
    {
        public Quaternion rotation;
        [Tooltip("Value between 0 to 10, where value less than 1 reduce the camera speed.")]
        [Range(0f, 10f)]
        public float speedFactor = 1;
    }
}
