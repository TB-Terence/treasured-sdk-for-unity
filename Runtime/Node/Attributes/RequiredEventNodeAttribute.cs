using System;

namespace Treasured.UnitySdk
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class RequiredEventNodeAttribute : Attribute
    {
        public Type OwnerType { get; set; }
        public Type[] Types { get; set; }

        public RequiredEventNodeAttribute(Type ownerType, params Type[] types)
        {
            OwnerType = ownerType;
            Types = types;
        }
    }
}
