using Newtonsoft.Json;
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
            return objectType == typeof(ActionCollection);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is ActionCollection collection)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var action in collection)
                {
                    Type type = action.GetType();
                    APIAttribute attribute = type.GetCustomAttributes<APIAttribute>().FirstOrDefault();
                    if (attribute == null)
                    {
                        continue;
                    }
                    sb.AppendLine($"{(attribute.IsAsync ? "await " : "")}{attribute.Domain}.{attribute.MethodName}({GetArgumentStrings(action.GetArguments())})");
                }
                writer.WriteValue(sb.ToString());
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
                    sb.Append(arg.ToString());
                }
                if (isString) sb.Append('\"');
            }
            return sb.ToString();
        }
    }
}