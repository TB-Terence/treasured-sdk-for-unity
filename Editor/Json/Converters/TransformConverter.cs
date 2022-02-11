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
                Vector3 rotation = transform.eulerAngles;
                Vector3 scale = transform.localScale;
                if (TransformConverter.ConvertToThreeJsSpace)
                {
                    position.x = -position.x;
                    rotation = Mathf.Deg2Rad * rotation;
                    rotation.y = -rotation.y;
                    scale.x = -scale.x;
                    scale.z = -scale.z;
                }
                writer.WriteStartObject();
                writer.WritePropertyName(nameof(transform.position));
                serializer.Serialize(writer, position);
                writer.WritePropertyName(nameof(transform.rotation));
                serializer.Serialize(writer, rotation);
                writer.WritePropertyName("scale");
                serializer.Serialize(writer, scale);
                writer.WriteEndObject();
            }
        }
    }
}