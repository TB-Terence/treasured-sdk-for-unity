using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Treasured.UnitySdk
{
    internal class TreasuredObjectConverter : JsonConverter
    {
        public override bool CanRead => false;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TreasuredObject) || objectType.GetElementType() == typeof(TreasuredObject) || (objectType.GenericTypeArguments.Length == 1 && objectType.GenericTypeArguments[0] == typeof(TreasuredObject));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is TreasuredObject to)
            {
                writer.WritePropertyName("id");
                writer.WriteValue(to.Id);
            }
            else if (value.GetType().GetElementType() == typeof(TreasuredObject) || (value.GetType().GenericTypeArguments.Length == 1 && value.GetType().GenericTypeArguments[0] == typeof(TreasuredObject)))
            {
                writer.WriteStartArray();
                foreach (var obj in (IList<TreasuredObject>)value)
                {
                    writer.WriteValue(obj.Id);
                }
                writer.WriteEndArray();
            }
        }
    }
}