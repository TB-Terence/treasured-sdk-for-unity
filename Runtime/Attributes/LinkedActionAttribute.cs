using System;
using System.Linq;

namespace Treasured.UnitySdk
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class LinkedActionAttribute : Attribute
    {
        public Type[] Types { get; private set; }

        public LinkedActionAttribute(params Type[] types)
        {
            Types = types.Where(type => typeof(ScriptableAction).IsAssignableFrom(type)).ToArray();
        }
    }
}
