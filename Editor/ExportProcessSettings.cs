using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace Treasured.UnitySdk
{
    internal class ExportProcessSettings
    {
        private static bool s_initialized = false;
        private static Dictionary<Type, ExportProcessSettings> s_instances = new Dictionary<Type, ExportProcessSettings>();
        public static IEnumerable<ExportProcessSettings> Instances => s_instances.Values;
        public bool Enabled { get; set; }
        public bool ShowInEditor { get; set; }
        public bool Expanded { get; set; }
        public string DisplayName { get; set; }
        public ExportProcess Instance { get; private set; }

        [InitializeOnLoadMethod]
        static void Init()
        {
            if (s_initialized)
            {
                return;
            }
            var types = Assembly.GetExecutingAssembly().GetTypes().Where(x => !x.IsAbstract && typeof(ExportProcess).IsAssignableFrom(x)).ToArray();
            foreach (var type in types)
            {
                if (s_instances.ContainsKey(type))
                {
                    continue;
                }
                ExportProcess instance = (ExportProcess)Activator.CreateInstance(type);
                var attribute = type.GetCustomAttribute<ExportProcessSettingsAttribute>();
                var settings = new ExportProcessSettings
                {
                    Instance = instance
                };
                if (attribute != null)
                {
                    settings.Enabled = attribute.EnabledByDefault;
                    settings.ShowInEditor = attribute.ShowInEditor;
                    settings.Expanded = attribute.ExpandedByDefault;
                    if (attribute.DisplayName == null)
                    {
                        string typeName = type.Name;
                        if (typeName.EndsWith("ExportProcess"))
                        {
                            typeName = typeName.Substring(0, typeName.Length - 13);
                        }
                        settings.DisplayName = ObjectNames.NicifyVariableName(typeName);
                    }
                    else
                    {
                        settings.DisplayName = attribute.DisplayName;
                    }
                }
                s_instances.Add(type, settings);
            }
            s_initialized = true;
        }
    }
}
