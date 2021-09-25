using Newtonsoft.Json;
using System;
using UnityEngine;

namespace Treasured.UnitySdk
{
    internal class HexColorConverter : JsonConverter<Color>
    {
        private readonly bool includeAlpha;

        public HexColorConverter(bool includeAlpha = false)
        {
            this.includeAlpha = includeAlpha;
        }

        public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            ColorUtility.TryParseHtmlString(reader.ReadAsString(), out Color color);
            return color;
        }

        public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
        {
            writer.WriteValue($"#{(includeAlpha ? ColorUtility.ToHtmlStringRGBA(value) : ColorUtility.ToHtmlStringRGB(value))}");
        }
    }
}