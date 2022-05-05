using System;
using System.Linq;
using UnityEditor;

namespace Treasured.UnitySdk
{
    internal static class DataValidator
    {
        public static void ValidateMap(TreasuredMap map)
        {
            var objects = map.GetComponentsInChildren<TreasuredObject>();
            FixDuplicateIds(objects);
        }

        public static void FixDuplicateIds(TreasuredObject[] objects)
        {
            var ids = objects.GroupBy(o => o.Id).Where(g => g.Count() > 1)
              .Select(y => new { Elements = y.Skip(1), Id = y.Key, Counter = y.Count() })
              .ToList();
            foreach (var id in ids)
            {
                int count = id.Elements.Count();
                for (int i = 0; i < count; i++)
                {
                    TreasuredObject obj = id.Elements.ElementAt(i);
                    obj.Id = Guid.NewGuid().ToString();
                    EditorUtility.SetDirty(obj);
                }
            }
        }
    }
}
