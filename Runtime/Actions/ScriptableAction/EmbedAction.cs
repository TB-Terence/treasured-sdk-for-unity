namespace Treasured.UnitySdk.Actions
{
    [API("embed")]
    public class EmbedAction : ScriptableAction
    {
        [Url]
        public string src;
        public EmbedPosition position;
    }
}
