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
                writer.WriteStartObject();
                writer.WritePropertyName(nameof(camera.transform.position));
                serializer.Serialize(writer, ThreeJsTransformConverter.ToThreeJsPosition(camera.transform));
                writer.WritePropertyName(nameof(camera.transform.rotation));
                if (ThreeJsTransformConverter.ShouldConvertToThreeJsTransform)
                {
                    serializer.Serialize(writer, camera.transform.rotation);
                }
                else
                {
                    serializer.Serialize(writer, ThreeJsTransformConverter.ToThreeJsEulerAngles(camera.transform));
                }
                writer.WriteEndObject();
            }
        }
    }
}