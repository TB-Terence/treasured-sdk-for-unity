using System;

namespace Treasured.UnitySdk
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ControlAttribute : Attribute
    {
        public string name;

        public ControlAttribute(string name)
        {
            this.name = name;
        }
    }
}
