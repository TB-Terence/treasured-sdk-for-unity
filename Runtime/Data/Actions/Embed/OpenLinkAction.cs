using UnityEngine;

namespace Treasured.UnitySdk
{
    /// <summary>
    /// Action that opens a link embed page.
    /// </summary>
    public class OpenLinkAction : EmbedAction
    {
        [SerializeField]
        [TextArea(3, 3)]
        private string _src;
        public string Src { get => _src; set => _src = value; }
    }
}
