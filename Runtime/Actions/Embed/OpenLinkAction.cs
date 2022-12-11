using UnityEngine;
using UnityEngine.Serialization;

namespace Treasured.UnitySdk
{
    /// <summary>
    /// Action that opens a link embed page.
    /// </summary>
    public class OpenLinkAction : Action
    {
        [FormerlySerializedAs("_position")]
        public EmbedPosition position = EmbedPosition.TopRight;

        [Url]
        [FormerlySerializedAs("_src")]
        public string src;

        internal override ScriptableAction ConvertToScriptableAction()
        {
            EmbedAction action = new EmbedAction();
            action.src = src;
            return action;
        }
    }
}
