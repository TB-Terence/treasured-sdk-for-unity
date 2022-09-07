using UnityEngine;

namespace Treasured.UnitySdk
{
    [API("setCameraRotation")]
    public class SetCameraRotationAction : ScriptableAction
    {
        public Quaternion rotation;

        public override object[] GetArguments()
        {
            return new object[] { rotation };
        }
    }
}
