using UnityEngine.Serialization;

namespace Treasured.UnitySdk
{
    /// <summary>
    /// Action that opens a link embed page.
    /// </summary>
    public class OpenLinkAction : EmbedAction
    {
        [Url]
        [FormerlySerializedAs("_src")]
        public string src;
    }
}
