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
            foreach (var obj in objects)
            {
                ValidateObject(obj);
            }
        }

        public static void ValidateObject(TreasuredObject obj)
        {
            if (obj is Hotspot hotspot && hotspot.Camera == null)
            {
                throw new ContextException("Missing reference", $"Camera Transform is not assigned for {obj.name}.", obj);
            }
            foreach (var group in obj.OnClick)
            {
                foreach (var action in group.Actions)
                {
                    if (action is SelectObjectAction soa)
                    {
                        if (soa.Target == null || (soa.Target != null && !soa.Target.gameObject.activeSelf))
                        {
                            throw new ContextException("Missing reference", $"The target for Select-Object action is inactive OR is not assigned for {obj.name}.", obj);
                        }
                        else if (soa.Target.GetComponentInParent<TreasuredMap>() != obj.GetComponentInParent<TreasuredMap>())
                        {
                            throw new ContextException("Invalid reference", $"The target set for Select-Object action does not belong to the same map.", obj);
                        }
                    }
                }
            }
            foreach (var group in obj.OnHover)
            {
                foreach (var action in group.Actions)
                {
                    if (action is SelectObjectAction soa)
                    {
                        if (soa.Target == null || (soa.Target != null && !soa.Target.gameObject.activeSelf))
                        {
                            throw new ContextException("Missing reference", $"The target for OnHover-Object action is inactive OR is not assigned for {obj.name}.", obj);
                        }
                        else if (soa.Target.GetComponentInParent<TreasuredMap>() != obj.GetComponentInParent<TreasuredMap>())
                        {
                            throw new ContextException("Invalid reference", $"The target set for OnHover-Object action does not belong to the same map.", obj);
                        }
                    }
                }
            }
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
