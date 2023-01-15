using Newtonsoft.Json;
using System;

namespace Treasured.UnitySdk
{
    internal class ShowPreviewActionConverter : JsonConverter
    {
        public override bool CanRead => false;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ShowPreviewAction) || objectType == typeof(Actions.ShowPreviewAction) || objectType == typeof(Treasured.Actions.ShowPreviewNode);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is Actions.ShowPreviewAction newAction)
            {
                if (!newAction.enabled)
                {
                    return;
                }
                writer.WriteStartObject();
                writer.WritePropertyName("id");
                writer.WriteValue(newAction.Id);
                writer.WritePropertyName("targetId");
                writer.WriteValue(newAction.target.IsNullOrNone() ? "" : newAction.target.Id);
                writer.WriteEndObject();
            }
            else if (value is ShowPreviewAction action)
            {
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
                    writer.WriteEndObject();
                }
            }
            else if (value is Treasured.Actions.ShowPreviewNode node)
            {
                if (node.target.IsNullOrNone())
                {
                    writer.WriteNull();
                }
                else
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("id");
                    writer.WriteValue(node.Id);
                    writer.WritePropertyName("type");
                    writer.WriteValue(node.Type);
                    writer.WritePropertyName("targetId");
                    writer.WriteValue(node.target.Id);
                    writer.WriteEndObject();
                }
            }
        }
    }
}