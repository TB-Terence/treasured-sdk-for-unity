namespace Treasured.UnitySdk
{
    internal static class JsonValidator
    {
        public static void ValidateMap(TreasuredMap map)
        {
            if (string.IsNullOrWhiteSpace(map.Title))
            {
                throw new System.MissingFieldException("The title field is missing.");
            }
            if (string.IsNullOrWhiteSpace(map.Description))
            {
                throw new System.MissingFieldException("The description field is missing.");
            }
            foreach (var obj in map.GetComponentsInChildren<TreasuredObject>())
            {
                ValidateObject(obj);
            }
        }

        public static void ValidateObject(TreasuredObject obj)
        {
            foreach (var action in obj.OnSelected)
            {
                if (action is SelectObjectAction soa && (soa.Target == null || (soa.Target != null && !soa.Target.gameObject.activeSelf)))
                {
                    throw new TargetNotAssignedException($"The target for Select-Object action is inactive OR is not assigned for {obj.name}.", obj);
                }
            }
        }
    }
}
