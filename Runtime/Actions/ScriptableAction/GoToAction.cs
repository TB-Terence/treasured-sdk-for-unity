namespace Treasured.UnitySdk
{
    [API("goto")]
    public class GoToAction : ScriptableAction
    {
        public Hotspot target;
        public string message;
    }
}
