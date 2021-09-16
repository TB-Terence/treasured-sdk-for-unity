using System;
using UnityEngine;

namespace Treasured.UnitySdk
{
    public class IndentAttribute : PropertyAttribute
    {
        public int IndentLevel { get; set; }

        public IndentAttribute(int indentLevel)
        {
            IndentLevel = indentLevel;
        }
    }
}
