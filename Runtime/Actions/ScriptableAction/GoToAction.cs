using UnityEngine;

namespace Treasured.UnitySdk
{
    [API("goTo")]
    [CreateActionGroup(typeof(SetCameraRotationAction))]
    [CreateActionGroup(typeof(PanAction))]
    public class GoToAction : ScriptableAction
    {
        public Hotspot target;
    }
}
