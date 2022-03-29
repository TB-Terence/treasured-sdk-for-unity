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
                if (transform == null)
                {
                    writer.WriteNull();
                }
                else
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName(nameof(transform.position));
                    serializer.Serialize(writer, ThreeJsTransformConverter.ToThreeJsPosition(transform));
                    writer.WritePropertyName(nameof(transform.rotation));
                    if (ThreeJsTransformConverter.ShouldConvertToThreeJsTransform)
                    {
                        serializer.Serialize(writer, ThreeJsTransformConverter.ToThreeJsQuaternion(transform));
                    }
                    else
                    {
                        serializer.Serialize(writer, ThreeJsTransformConverter.ToThreeJsEulerAngles(transform));
                    }
                    writer.WritePropertyName("scale");
                    serializer.Serialize(writer, ThreeJsTransformConverter.ToThreeJsScale(transform));
                    writer.WriteEndObject();
                }
            }
        }
    }
}