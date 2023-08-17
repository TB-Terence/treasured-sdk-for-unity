using System;

namespace Treasured.UnitySdk
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ComponentCardAttribute : Attribute
    {
        public string name;
        public string description;
        public string iconName;
        public string helpUrl;

        public ComponentCardAttribute(string name, string description, string iconName) : this(name, description, iconName, string.Empty)
        {

        }

        public ComponentCardAttribute(string name, string description, string iconName, string helpUrl)
        {
            this.name = name;
            this.description = description;
            this.iconName = iconName;
            this.helpUrl = helpUrl;
        }
    }
}
