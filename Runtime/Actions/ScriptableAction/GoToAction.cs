namespace Treasured.UnitySdk
{
    [API("goto")]
    [LinkedAction(typeof(SetCameraRotationAction))]
    [LinkedAction(typeof(PanAction))]
    public class GoToAction : ScriptableAction
    {
        public Hotspot target;
        public string message;
    }
}
