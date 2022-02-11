using Newtonsoft.Json;
using System;
using UnityEngine;

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
                Vector3 position = hitbox.transform.position;
                Vector3 rotation = hitbox.transform.eulerAngles;
                Vector3 scale = hitbox.transform.localScale;
                if (TransformConverter.ConvertToThreeJsSpace)
                {
                    position.x = -position.x;
                    rotation = Mathf.Deg2Rad * rotation;
                    rotation.y = -rotation.y;
                    scale.x = -scale.x;
                    scale.z = -scale.z;
                }
                writer.WriteStartObject();
                writer.WritePropertyName(nameof(hitbox.transform.position));
                serializer.Serialize(writer, position); // use custom Vector3Converter
                writer.WritePropertyName(nameof(hitbox.transform.rotation));
                serializer.Serialize(writer, rotation);
                writer.WritePropertyName("size");
                serializer.Serialize(writer, scale);
                writer.WriteEndObject();
            }
        }
    }
}