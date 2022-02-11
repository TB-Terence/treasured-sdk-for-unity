using Newtonsoft.Json;
using System;
using UnityEngine;

namespace Treasured.UnitySdk
{
    internal class HotspotCameraConverter : JsonConverter
    {
        public override bool CanRead => false;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(HotspotCamera);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is HotspotCamera camera)
            {
                Vector3 position = camera.transform.position;
                Vector3 rotation = camera.transform.eulerAngles;
                if (TransformConverter.ConvertToThreeJsSpace)
                {
                    position.x = -position.x;
                    rotation = Mathf.Deg2Rad * rotation;
                    rotation.y = -rotation.y;
                }
                writer.WriteStartObject();
                writer.WritePropertyName(nameof(camera.transform.position));
                serializer.Serialize(writer, position);
                writer.WritePropertyName(nameof(camera.transform.rotation));
                serializer.Serialize(writer, rotation);
                writer.WriteEndObject();
            }
        }
    }
}