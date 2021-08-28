using Newtonsoft.Json;
using Treasured.UnitySdk;
using System;
using Newtonsoft.Json.Linq;

namespace Treasured.SDKEditor
{
    internal class TreasuredMapConverter : JsonConverter<TreasuredMap>
    {
        public override TreasuredMap ReadJson(JsonReader reader, Type objectType, TreasuredMap existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            serializer.Populate(reader, existingValue);
            return existingValue;
        }

        public override void WriteJson(JsonWriter writer, TreasuredMap value, JsonSerializer serializer)
        {
            //serializer.Serialize(writer, value, typeof());
            //writer.WriteStartObject();

            //WriteProperty(writer, "version", "0.4.0");
            //WriteProperty(writer, nameof(value.Title), value.Title);
            //WriteProperty(writer, nameof(value.Description), value.Description);
            //WriteProperty(writer, nameof(value.Loop), value.Loop);
            //WriteProperty(writer, nameof(value.Format), value.Format);
            //WriteProperty(writer, nameof(value.Quality), value.Quality);
            //WritePropertyArray(writer, nameof(value.Hotspots), value.Hotspots);
            //WritePropertyArray(writer, nameof(value.Interactables), value.Interactables);

            //writer.WriteEndObject();
        }

        private void WriteProperty(JsonWriter writer, string name, object value)
        {
            writer.WritePropertyName(name);
            writer.WriteValue(value);
        }

        private void WritePropertyArray(JsonWriter writer, string name, TreasuredObject[] values)
        {
            writer.WritePropertyName(name);
            writer.WriteStartArray();
            foreach (var value in values)
            {
                writer.WriteValue(value);
            }
            writer.WriteEndArray();
        }
    }
}
