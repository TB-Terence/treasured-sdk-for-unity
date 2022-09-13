using UnityEngine;

namespace Treasured.UnitySdk
{
    [API("setCameraRotation")]
    public class SetCameraRotationAction : ScriptableAction
    {
        public Quaternion rotation;
        [Range(1, 10)]
        public int speedFactor = 1;
    }
}
