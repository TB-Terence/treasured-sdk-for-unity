namespace Treasured.UnitySdk.Actions
{
    //[API("openLink")]
    //[API("embed")]
    public class EmbedAction : ScriptableAction
    {
        [Url]
        public string src;
        public WidgetPosition position;
    }
}
