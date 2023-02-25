using Newtonsoft.Json;
using System;

namespace Treasured.UnitySdk
{
    internal class GoToActionConverter : JsonConverter
    {
        public override bool CanRead => false;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(GoToAction);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is GoToAction action)
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
                    writer.WritePropertyName("type");
                    writer.WriteValue(action.Type);
                    writer.WritePropertyName("targetId");
                    writer.WriteValue(action.target.Id);
                    writer.WritePropertyName("targetType");
                    writer.WriteValue("hotspot");
                    writer.WriteEndObject();
                }
            }
        }
    }
}