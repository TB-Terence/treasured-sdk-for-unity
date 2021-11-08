using UnityEngine;

namespace Treasured.UnitySdk
{
    /// <summary>
    /// Action that runs a custom embedded code.
    /// </summary>
    public class CustomCodeAction : EmbedAction
    {
        [SerializeField]
        [TextArea(9, 9)]
        private string _src;
        public string Src { get => _src; set => _src = value; }
    }
}