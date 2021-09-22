using UnityEngine;

namespace Treasured.UnitySdk
{
    public class ShowTextAction : ActionBase
    {
        public enum TextStyles
        {
            None,
            Dialogue,
            Fade
        }

        [SerializeField]
        [TextArea(5, 5)]
        private string _content;

        [SerializeField]
        private TextStyles _style = TextStyles.Dialogue;

        public string Content { get => _content; set => _content = value; }
        public TextStyles Style { get => _style; set => _style = value; }
    }
}
