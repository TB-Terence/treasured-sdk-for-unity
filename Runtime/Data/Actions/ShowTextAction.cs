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
        [TextArea(5, 5)]
        public string content;
        public TextStyles style;
    }
}
