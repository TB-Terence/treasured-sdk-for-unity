namespace Treasured.UnitySdk.Validation
{
    public struct ValidationResult
    {
        public struct ValidationResolver
        {
            public string text;
            public System.Action onResolve;
        }

        [System.Flags]
        public enum ValidationResultType
        {
            Info = 1,
            Warning = 2,
            Error = 4,
        }

        public string name;
        public string description;
        public ValidationResultType type;
        public ValidationResolver[] resolvers;
        /// <summary>
        /// Priority of the error/warning show up in the validation window. Lower number appear first.
        /// </summary>
        public int priority;
    }
}
