using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Treasured.UnitySdk
{
    [InitializeOnLoad]
    internal static class CacheUtils
    {
        private static Dictionary<string, TreasuredMap> maps = new Dictionary<string, TreasuredMap>();

        static CacheUtils()
        {
            CacheMaps();
        }

        static void CacheMaps()
        {

        }
    }
}
