using Newtonsoft.Json;
using System;

namespace Treasured.UnitySdk
{
    internal class HitboxConverter : JsonConverter
    {
        public override bool CanRead => false;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Hitbox);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is Hitbox hitbox)
            {
                writer.WriteStartObject();
                writer.WritePropertyName(nameof(hitbox.transform.position));
                serializer.Serialize(writer, hitbox.transform.position); // use custom Vector3Converter
                writer.WritePropertyName(nameof(hitbox.transform.rotation));
                serializer.Serialize(writer, hitbox.transform.eulerAngles);
                writer.WritePropertyName("size");
                serializer.Serialize(writer, hitbox.transform.localScale);
                writer.WriteEndObject();
            }
        }
    }
}