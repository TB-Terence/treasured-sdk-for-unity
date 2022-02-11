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
                Quaternion rotation = hitbox.transform.rotation;
                if (TransformConverter.ConvertToThreeJsSpace)
                {
                    // THREE.Euler order ZXY
                    position.z = -position.z;
                    Quaternion angle = Quaternion.Euler(0, 180, 0);
                    position = angle * hitbox.transform.position;
                    rotation = Quaternion.Euler(hitbox.transform.eulerAngles.x, -hitbox.transform.eulerAngles.y, hitbox.transform.eulerAngles.z);
                }
                writer.WriteStartObject();
                writer.WritePropertyName(nameof(hitbox.transform.position));
                serializer.Serialize(writer, position); // use custom Vector3Converter
                writer.WritePropertyName(nameof(hitbox.transform.rotation));
                serializer.Serialize(writer, rotation.eulerAngles);
                writer.WritePropertyName("size");
                serializer.Serialize(writer, hitbox.transform.localScale);
                writer.WriteEndObject();
            }
        }
    }
}