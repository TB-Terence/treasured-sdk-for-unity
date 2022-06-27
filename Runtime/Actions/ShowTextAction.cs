﻿using UnityEngine;
using UnityEngine.Serialization;

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

        [TextArea(3, 5)]
        [FormerlySerializedAs("_content")]
        public string content;
        [FormerlySerializedAs("_style")]
        public TextStyles style = TextStyles.Dialogue;

        /// <summary>
        /// Duration for the text in seconds.
        /// </summary>
        [Min(0)]
        public int duration;
    }
}
