using System;

namespace Treasured.UnitySdk
{
    internal class ButtonAttribute : Attribute
    {
        public string Text { get; set; }
        public ButtonAttribute()
        {

        }

        public ButtonAttribute(string text)
        {
            Text = text;
        }
    }
}
