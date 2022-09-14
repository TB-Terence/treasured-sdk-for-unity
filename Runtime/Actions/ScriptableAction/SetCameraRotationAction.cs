using Newtonsoft.Json;
using System;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [API("setCameraRotation")]
    public class SetCameraRotationAction : ScriptableAction
    {
        public enum Speed
        {
            x0_25,
            x0_5,
            x0_75,
            x1,
            x1_25,
            x1_5,
            x1_75,
            x2
        }
        public Quaternion rotation;
        [JsonIgnore]
        public Speed speed = Speed.x1;

        public float SpeedFactor
        {
            get
            {
                return (Array.IndexOf(Enum.GetValues(typeof(Speed)), speed) + 1) * 0.25f;
            }
        }
    }
}
