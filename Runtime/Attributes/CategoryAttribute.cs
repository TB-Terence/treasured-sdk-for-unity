using System;

namespace Treasured.UnitySdk
{
    [AttributeUsage(AttributeTargets.Class)]
    internal class CategoryAttribute : Attribute
    {
        public string Path { get; set; }
        public CategoryAttribute(string path)
        {
            this.Path = path;
        }
    }
}
