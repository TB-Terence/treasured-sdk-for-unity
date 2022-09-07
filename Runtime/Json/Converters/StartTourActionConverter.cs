using Newtonsoft.Json;
using System;

namespace Treasured.UnitySdk
{
    internal class StartTourActionConverter : JsonConverter
    {
        public override bool CanRead => false;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(StartTourAction);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is StartTourAction action)
            {
                if (!action.enabled)
                {
                    return;
                }
                if (action.target.IsNullOrNone())
                {
                    writer.WriteNull();
                }
                else
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("id");
                    writer.WriteValue(action.Id);
                    writer.WritePropertyName("tourId");
                    writer.WriteValue(action.target.Id);
                    writer.WriteEndObject();
                }
            }
        }
    }
}