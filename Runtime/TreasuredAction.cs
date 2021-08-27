using System;
using UnityEngine;

namespace Treasured.SDK
{
    public enum DisplayMode
    {
        Center,
        Left,
        Right,
        Fullscreen,
        NewTab
    }

    public enum Style
    {
        Dialogue,
        Fade
    }

    [Serializable]
    public sealed class TreasuredAction
    {
        [SerializeField]
        [HideInInspector]
        private string _id;
        [SerializeField]
        [StringSelector("openLink", "showText", "playAudio", "playVideo", "selectObject")]
        private string _type;
        [SerializeField]
        [TextArea]
        private string _src;
        [SerializeField]
        private string _targetId;
        [SerializeField]
        private DisplayMode _displayMode = DisplayMode.Right;
        [SerializeField]
        [TextArea(5, 5)]
        private string _content;
        [SerializeField]
        private Style _style;

        public string Id { get => _id; set => _id = value; }
        public string Type { get => _type; set => _type = value; }
        public string Src { get => _src; set => _src = value; }
        public string TargetId { get => _targetId; set => _targetId = value; }
        public DisplayMode DisplayMode { get => _displayMode; set => _displayMode = value; }
        public string Content { get => _content; set => _content = value; }
        public Style Style { get => _style; set => _style = value; }
    }
}