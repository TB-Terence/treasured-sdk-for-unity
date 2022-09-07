namespace Treasured.UnitySdk
{
    [API("button")]
    public class ButtonAction : ScriptableAction
    {
        public string text;

        public override object[] GetArguments()
        {
            return new object[] { text };
        }
    }
}
