using Newtonsoft.Json;
using System;

namespace Treasured.UnitySdk
{
    internal class IconAssetConverter : JsonConverter
    {
        public override bool CanRead => false;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IconAsset);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is IconAsset icon && !icon.IsNullOrNone())
            {
                writer.WriteValue(icon.name);
            }
            else
            {
                writer.WriteNull();
            }
        }
    }
}
