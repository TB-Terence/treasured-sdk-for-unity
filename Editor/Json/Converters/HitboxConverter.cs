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
                writer.WriteStartObject();
                writer.WritePropertyName(nameof(hitbox.transform.position));
                serializer.Serialize(writer, ThreeJsTransformConverter.ToThreeJsPosition(hitbox.transform));
                writer.WritePropertyName(nameof(hitbox.transform.rotation));
                if (ThreeJsTransformConverter.ShouldConvertToThreeJsTransform)
                {
                    serializer.Serialize(writer, ThreeJsTransformConverter.ToThreeJsQuaternion(hitbox.transform));
                }
                else
                {
                    serializer.Serialize(writer, ThreeJsTransformConverter.ToThreeJsEulerAngles(hitbox.transform));
                }
                writer.WritePropertyName("size");
                serializer.Serialize(writer, ThreeJsTransformConverter.ToThreeJsPosition(hitbox.transform));
                writer.WriteEndObject();
            }
        }
    }
}