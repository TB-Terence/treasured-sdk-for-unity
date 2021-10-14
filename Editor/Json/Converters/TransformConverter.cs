using Newtonsoft.Json;
using System;
using UnityEngine;

namespace Treasured.UnitySdk
{
    internal class TransformConverter : JsonConverter<Transform>
    {
        public override bool CanRead => false;

        public override Transform ReadJson(JsonReader reader, Type objectType, Transform existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return null;
        }

        public override void WriteJson(JsonWriter writer, Transform value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            #region Position
            writer.WritePropertyName(nameof(value.position));
            writer.WriteStartObject();
            writer.WritePropertyName(nameof(value.position.x));
            writer.WriteValue(value.position.x);
            writer.WritePropertyName(nameof(value.position.y));
            writer.WriteValue(value.position.y);
            writer.WritePropertyName(nameof(value.position.z));
            writer.WriteValue(value.position.z);
            writer.WriteEndObject();
            #endregion
            #region Rotation
            writer.WritePropertyName(nameof(value.rotation));
            writer.WriteStartObject();
            writer.WritePropertyName(nameof(value.eulerAngles.x));
            writer.WriteValue(value.eulerAngles.x);
            writer.WritePropertyName(nameof(value.eulerAngles.y));
            writer.WriteValue(value.eulerAngles.y);
            writer.WritePropertyName(nameof(value.eulerAngles.z));
            writer.WriteValue(value.eulerAngles.z);
            writer.WriteEndObject();
            #endregion
            writer.WriteEndObject();
        }
    }
}