using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Treasured.UnitySdk
{
    internal class ActionCollectionConverter : JsonConverter
    {
        public override bool CanRead => false;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ScriptableActionCollection);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is ScriptableActionCollection collection)
            {
                writer.WriteStartArray();
                foreach (var action in collection)
                {
                    if (!action.enabled)
                    {
                        continue;
                    }
                    writer.WriteStartObject();
                    writer.WritePropertyName("id");
                    writer.WriteValue(action.Id);
                    writer.WritePropertyName("function");
                    APIAttribute apiAttribute = GetType().GetCustomAttributes<APIAttribute>().FirstOrDefault();
                    string functionName = apiAttribute != null ? apiAttribute.FunctionName : action.Type;
                    writer.WriteValue(functionName);
                    writer.WritePropertyName("params");
                    JObject jAction = JObject.FromObject(action, serializer);
                    jAction.WriteTo(writer);
                    writer.WriteEndObject();
                }
                writer.WriteEndArray();
            }
        }

        private string GetArgumentStrings(object[] args)
        {
            if (args == null || args.Length == 0) return "";
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < args.Length; i++)
            {
                object arg = args[i];
                if (arg == null)
                {
                    sb.Append("null");
                    continue;
                }
                if (i > 0)
                {
                    sb.Append(", ");
                }
                bool isString = arg.GetType() == typeof(string);
                if(isString) sb.Append('\"');
                if (!isString && !arg.GetType().IsValueType)
                {
                    sb.Append("null");
                }
                else
                {
                    sb.Append(JsonConvert.SerializeObject(arg, Formatting.None, JsonExporter.JsonSettings));
                }
                if (isString) sb.Append('\"');
            }
            return sb.ToString();
        }
    }
}