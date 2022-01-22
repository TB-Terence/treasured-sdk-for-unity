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
            foreach (var obj in map.GetComponentsInChildren<TreasuredObject>())
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
