namespace Treasured.UnitySdk
{
    [API("goto")]
    [CreateActionGroup(typeof(SetCameraRotationAction))]
    [CreateActionGroup(typeof(PanAction))]
    public class GoToAction : ScriptableAction
    {
        public Hotspot target;
        public string message;
    }
}
