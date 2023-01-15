using Treasured.Actions;
using Treasured.UnitySdk;
using UnityEngine;

namespace Treasured.Actions
{
    [API("setCameraRotation")]
    public class SetCameraRotationNode : ActionNode
    {
        public Quaternion rotation;
        [Tooltip("Value between 0 to 100, where value less than 1 reduce the camera speed.")]
        public float speedFactor = 1;
    }
}
