using Newtonsoft.Json;
using System;

namespace Treasured.UnitySdk
{
    internal class StringConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return reader.Value.ToString();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(string.IsNullOrWhiteSpace((string)value) ? "" : value.ToString());
        }
    }
}
