using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace Treasured.UnitySdk.Analytics
{
    public static class Logger
    {
        private static Dictionary<string, object> s_items = new Dictionary<string, object>();
        public static object GetItem(string key)
        {
            if(s_items.TryGetValue(key, out object item))
            {
                return item;
            }
            return null;
        }

        public static IEnumerable<KeyValuePair<string, object>> GetItems()
        {
            return s_items;
        }

        public static void Log(string key, object value)
        {
            s_items[key] = value;
        }

        public static void Clear()
        {
            s_items.Clear();
        }

        public static void GenerateLogs(string directory)
        {
            string jsonPath = Path.Combine(directory, "logs.json").ToOSSpecificPath();
            string logs = JsonConvert.SerializeObject(s_items, Formatting.Indented, JsonExporter.JsonSettings);
            File.WriteAllText(jsonPath, logs);
        }
    }
}
