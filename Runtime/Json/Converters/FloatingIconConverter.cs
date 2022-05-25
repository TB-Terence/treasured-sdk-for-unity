using System;
using Newtonsoft.Json;
namespace Treasured.UnitySdk
{
    public class FloatingIconConverter : JsonConverter<FloatingIcon>
    {
        public override bool CanRead => false;
        public override FloatingIcon ReadJson(JsonReader reader, Type objectType, FloatingIcon existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return null;
        }

        public override void WriteJson(JsonWriter writer, FloatingIcon value, JsonSerializer serializer)
        {
            if (value.asset.IsNullOrNone())
            {
                writer.WriteNull();
            }
            else
            {
                writer.WriteStartObject();
                writer.WritePropertyName("icon");
                serializer.Serialize(writer, value.asset.name); // serialize only name, the actual svg data is under ./icons folder
                writer.WritePropertyName("transform");
                serializer.Serialize(writer, value.transform);
                writer.WriteEndObject();
            }
        }
    }
}
