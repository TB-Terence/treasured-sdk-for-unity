namespace Treasured.UnitySdk
{
    internal class ContextException : TreasuredException
    {
        public UnityEngine.Object Context { get; }
        public string PingText { get; }

        public ContextException(string title, string message, string okText, string pingText, UnityEngine.Object context) : base(title, message, okText)
        {
            this.PingText = pingText;
            this.Context = context;
        }

        public ContextException(string title, string message, string pingText, UnityEngine.Object context) : this(title, message, "Ok", pingText, context) { }
        public ContextException(string title, string message, UnityEngine.Object context) : this(title, message, "Ok", "Ping Object", context) { }
    }
}
