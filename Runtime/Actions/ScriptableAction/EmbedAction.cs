namespace Treasured.UnitySdk
{
    [API("embed")]
    public class EmbedAction : ScriptableAction
    {
        [Url]
        public string src;
        public string title;

        public override object[] GetArguments()
        {
            return new object[] { src, title };
        }
    }
}
