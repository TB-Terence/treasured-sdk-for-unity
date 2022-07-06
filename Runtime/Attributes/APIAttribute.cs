using System;

namespace Treasured.UnitySdk
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class APIAttribute : Attribute
    {
        public const char Separator = '.';
        public string Domain { get; set; } = "api";
        public string MethodName { get; set; }
        public string[] ArgumentGetters { get; set; }
        public bool IsAsync { get; set; } = true;

        public APIAttribute(string methodName)
        {
            this.MethodName = methodName;
        }

        public APIAttribute(string domain, string methodName) : this(methodName)
        {
            this.Domain = domain;
        }
    }
}
