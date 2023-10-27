using System;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    internal class ExportIfAttribute : Attribute
    {
        public string Getter { get; private set; }

        public ExportIfAttribute(string getter)
        {
            Getter = getter;
        }
    }
}
