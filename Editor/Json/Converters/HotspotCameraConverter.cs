using Newtonsoft.Json;
using System;

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
                serializer.Serialize(writer, camera.transform.position);
                writer.WritePropertyName(nameof(camera.transform.rotation));
                serializer.Serialize(writer, camera.transform.eulerAngles);
                writer.WriteEndObject();
            }
        }
    }
}