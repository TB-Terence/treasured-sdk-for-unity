using System;

namespace Treasured.UnitySdk
{
    internal class TreasuredException : Exception
    {
        public string Title { get; }
        public string OkText { get; }

        public TreasuredException(string title, string message, string okText) : base(message)
        {
            this.Title = title;
            this.OkText = okText;
        }

        public TreasuredException(string title, string message) : this(title, message, "Ok") { }
    }
}
