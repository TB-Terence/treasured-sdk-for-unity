using System;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property |
                    AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = true)]
    internal class UrlAttribute : PropertyAttribute
    {
    }
}
