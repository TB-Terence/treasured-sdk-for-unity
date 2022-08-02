namespace Treasured.UnitySdk
{
    [API("goto")]
    public class GoToAction : ScriptableAction
    {
        public Hotspot target;
        public string message;
        public override object[] GetArguments()
        {
            return new object[] { target?.Id };
        }
    }
}
