namespace Treasured.UnitySdk.Validation
{
    public struct ValidationResult
    {
        public enum ValidationResultType
        {
            Warning,
            Error
        }

        public string name;
        public string description;
        public UnityEngine.Object target;
        public UnityEngine.Object[] targets;
        public ValidationResultType type;
    }
}
