using UnityEngine;

namespace Treasured.UnitySdk
{
    /// <summary>
    /// Action that show text on screen.
    /// </summary>
    public class ShowTextAction : Action
    {
        public enum TextStyles
        {
            None,
            Dialogue,
            Fade
        }

        [SerializeField]
        [TextArea(3, 5)]
        private string _content;

        [SerializeField]
        private TextStyles _style = TextStyles.Dialogue;

        /// <summary>
        /// Duration for the text in seconds.
        /// </summary>
        [Min(0)]
        public int duration;

        public string Content { get => _content; set => _content = value; }
        public TextStyles Style { get => _style; set => _style = value; }
    }
}
