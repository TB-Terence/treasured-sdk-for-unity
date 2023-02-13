using System;

namespace Treasured.UnitySdk
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class GroupAttribute : Attribute
    {
        public string GroupName { get; private set; }
        public GroupAttribute()
        {

        }

        public GroupAttribute(string groupName)
        {
            this.GroupName = groupName;
        }
    }
}
