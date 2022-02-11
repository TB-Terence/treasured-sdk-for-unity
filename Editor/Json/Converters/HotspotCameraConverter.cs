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
                Quaternion rotation = camera.transform.rotation;
                if (TransformConverter.ConvertToThreeJsSpace)
                {
                    // THREE.Euler order ZXY
                    position.z = -position.z;
                    Quaternion angle = Quaternion.Euler(0, 180, 0);
                    position = angle * camera.transform.position;
                    rotation = Quaternion.Euler(camera.transform.eulerAngles.x, -camera.transform.eulerAngles.y, camera.transform.eulerAngles.z);
                }
                writer.WriteStartObject();
                writer.WritePropertyName(nameof(camera.transform.position));
                serializer.Serialize(writer, position);
                writer.WritePropertyName(nameof(camera.transform.rotation));
                serializer.Serialize(writer, rotation.eulerAngles);
                writer.WriteEndObject();
            }
        }
    }
}