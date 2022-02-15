using Newtonsoft.Json;
using System;
using UnityEngine;

namespace Treasured.UnitySdk
{
    internal class TransformConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Transform);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is Transform transform)
            {
                writer.WriteStartObject();
                writer.WritePropertyName(nameof(transform.position));
                serializer.Serialize(writer, ThreeJsTransformConverter.ToThreeJsPosition(transform.position));
                writer.WritePropertyName(nameof(transform.rotation));
                serializer.Serialize(writer, ThreeJsTransformConverter.ToThreeJsRotation(transform.rotation));
                writer.WritePropertyName("scale");
                serializer.Serialize(writer, ThreeJsTransformConverter.ToThreeJsScale(transform.localScale));
                writer.WriteEndObject();
            }
        }
    }
}