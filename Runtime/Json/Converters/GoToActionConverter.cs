using Newtonsoft.Json;
using System;
using Treasured.Actions;

namespace Treasured.UnitySdk
{
    internal class GoToActionConverter : JsonConverter
    {
        public override bool CanRead => false;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(GoToAction) || objectType == typeof(GoToNode);
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
                    writer.WritePropertyName("hotspotId");
                    writer.WriteValue(action.target.Id);
                    writer.WritePropertyName("message");
                    writer.WriteValue(action.message);
                    writer.WriteEndObject();
                }
            }
            else if(value is GoToNode node)
            {
                if (node.target.IsNullOrNone())
                {
                    writer.WriteNull();
                }
                else
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("hotspotId");
                    writer.WriteValue(node.target.Id);
                    writer.WritePropertyName("message");
                    writer.WriteValue(node.message);
                    writer.WriteEndObject();
                }
            }
        }
    }
}