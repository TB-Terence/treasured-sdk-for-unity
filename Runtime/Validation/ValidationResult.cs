namespace Treasured.UnitySdk
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
        public UnityEngine.Object context;
        public ValidationResultType type;
    }
}
