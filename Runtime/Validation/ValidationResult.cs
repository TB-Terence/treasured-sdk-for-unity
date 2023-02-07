namespace Treasured.UnitySdk.Validation
{
    public struct ValidationResult
    {
        public struct ValidationResolver
        {
            public string text;
            public System.Action onResolve;
        }

        public enum ValidationResultType
        {
            Warning,
            Error
        }

        public string name;
        public string description;
        public ValidationResultType type;
        public ValidationResolver[] resolvers;
    }
}
