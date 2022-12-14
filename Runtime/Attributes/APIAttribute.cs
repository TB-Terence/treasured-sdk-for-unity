using System;

namespace Treasured.UnitySdk
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class APIAttribute : Attribute
    {
        public const char Separator = '.';
        public string Domain { get; set; } = "api";
        public string FunctionName { get; set; }
        public bool IsAsync { get; set; } = true;

        public APIAttribute(string methodName)
        {
            this.FunctionName = methodName;
        }

        public APIAttribute(string domain, string methodName) : this(methodName)
        {
            this.Domain = domain;
        }
    }
}
