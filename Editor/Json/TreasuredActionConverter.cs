using Newtonsoft.Json;
using System;

namespace Treasured.SDK
{
    internal class TreasuredActionConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TreasuredAction);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            TreasuredAction result = new TreasuredAction();
            serializer.Populate(reader, result);
            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            TreasuredAction action = (TreasuredAction)value;
            writer.WriteStartObject();
            WritePropertyValue(writer, "id", action.Id);
            WritePropertyValue(writer, "type", action.Type);
            switch (action.Type)
            {
                case "OpenLink":
                case "PlayAudio":
                case "PlayVideo":
                    WritePropertyValue(writer, "src", action.Src);
                    WritePropertyValue(writer, "displayMode", action.DisplayMode);
                    break;
                case "ShowText":
                    WritePropertyValue(writer, "content", action.Content);
                    break;
                case "SelectObject":
                    WritePropertyValue(writer, "targetId", action.TargetId);
                    break;
            }
            writer.WriteEndObject();
        }

        private void WritePropertyValue(JsonWriter writer, string name, object value)
        {
            writer.WritePropertyName(name);
            writer.WriteValue(value);
        }
    }
}
