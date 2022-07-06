namespace Treasured.UnitySdk
{
    [Category("v2")]
    [API("embed")]
    public class EmbedAction : Action
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
