namespace Treasured.UnitySdk.Actions
{
    [API("openLink")]
    public class EmbedAction : ScriptableAction
    {
        [Url]
        public string src;
        public EmbedPosition position;
    }
}
