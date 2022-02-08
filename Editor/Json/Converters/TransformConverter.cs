using Newtonsoft.Json;
using System;
using UnityEngine;

namespace Treasured.UnitySdk
{
    internal class TransformConverter : JsonConverter
    {
        public static bool ConvertToThreeJsSpace = false;

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
                Vector3 position = transform.position;
                Quaternion rotation = transform.rotation;
                if (TransformConverter.ConvertToThreeJsSpace)
                {
                    // THREE.Euler order ZXY
                    position.z = -position.z;
                    Quaternion angle = Quaternion.Euler(0, 180, 0);
                    position = angle * transform.position;
                    rotation = Quaternion.Euler(transform.eulerAngles.x, -transform.eulerAngles.y, transform.eulerAngles.z);
                }
                writer.WriteStartObject();
                writer.WritePropertyName(nameof(transform.position));
                serializer.Serialize(writer, position);
                writer.WritePropertyName(nameof(transform.rotation));
                serializer.Serialize(writer, rotation);
                writer.WritePropertyName("scale");
                serializer.Serialize(writer, transform.localScale);
                writer.WriteEndObject();
            }
        }
    }
}