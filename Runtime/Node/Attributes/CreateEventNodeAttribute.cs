using System;

namespace Treasured.UnitySdk
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class CreateEventNodeAttribute : Attribute
    {
        public Type OwnerType { get; set; }
        public Type[] Types { get; set; }

        public CreateEventNodeAttribute(Type ownerType, params Type[] types)
        {
            OwnerType = ownerType;
            Types = types;
        }
    }
}
