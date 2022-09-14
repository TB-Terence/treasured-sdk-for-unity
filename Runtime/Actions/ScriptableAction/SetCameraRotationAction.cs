using Newtonsoft.Json;
using System;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [API("setCameraRotation")]
    public class SetCameraRotationAction : ScriptableAction
    {
        public Quaternion rotation;
        [Tooltip("Value between 0 to 100, where value less than 1 reduce the camera speed.")]
        public float speedFactor = 1;
    }
}
