using System;

namespace Treasured.UnitySdk
{
    [AttributeUsage(AttributeTargets.Class)]
    internal class ExportProcessSettingsAttribute : Attribute
    {
        public bool EnabledByDefault { get; set; } = true;
        public bool ExpandedByDefault { get; set; } = true;
        public string DisplayName { get; set; }

    }
}
