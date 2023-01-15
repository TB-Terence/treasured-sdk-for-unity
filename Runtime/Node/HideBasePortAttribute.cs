using System;

namespace Treasured
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class HideBasePortAttribute : Attribute
    {
        public string PortName { get; set; }

        public HideBasePortAttribute(string portName)
        {
            PortName = portName;
        }
    }
}
