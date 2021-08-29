using UnityEngine;
using UnityEditor;
using Treasured.UnitySdk;

namespace Treasured.UnitySdk.Editor
{
    internal static class ExtensionMethods
    {
        public static void Populate(this TreasuredMap map, TreasuredMapData data)
        {
            if (map == null)
            {
                return;
            }
            map.Data = data;
        }
    }
}
