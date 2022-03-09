using System.Linq;
using System.Text;

namespace Treasured.UnitySdk
{
    internal static class DataValidator
    {
        public static void ValidateMap(TreasuredMap map)
        {
            if (string.IsNullOrWhiteSpace(map.Author))
            {
                throw new TreasuredException("Missing Field", "The author field is missing.");
            }
            if (string.IsNullOrWhiteSpace(map.Title))
            {
                throw new TreasuredException("Missing Field", "The title field is missing.");
            }
            if (string.IsNullOrWhiteSpace(map.Description))
            {
                throw new TreasuredException("Missing Field", "The description field is missing.");
            }
            var objects = map.GetComponentsInChildren<TreasuredObject>();
            var ids = objects.GroupBy(o => o.Id).Where(g => g.Count() > 1)
              .Select(y => new { Elements = y.Skip(1), Id = y.Key, Counter = y.Count() })
              .ToList();
            bool hasDuplicates = false;
            StringBuilder sb = new StringBuilder();
            sb.Append("Duplicated id for the following object(s): ");
            foreach (var id in ids)
            {
                if (id.Counter > 1)
                {
                    hasDuplicates = true;
                    int count = id.Elements.Count();
                    for (int i = 0; i < count; i++)
                    {
                        sb.Append(id.Elements.ElementAt(i).name + (i != count - 1 ? ", " : ""));
                    }
                }
            }
            if (hasDuplicates)
            {
                throw new TreasuredException("Duplicated Ids", sb.ToString());
            }
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
    }
}
