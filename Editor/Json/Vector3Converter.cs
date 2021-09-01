using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using UnityEngine;

namespace Treasured.SDK
{
    internal class Vector3Converter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Vector3);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Vector3 result = new Vector3();
            JToken token = JToken.Load(reader);
            if(token["x"] != null)
            {
                result.x = (float)token["x"];
            }
            if (token["y"] != null)
            {
                result.y = (float)token["y"];
            }
            if (token["z"] != null)
            {
                result.z = (float)token["z"];
            }
            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Vector3 vector3 = (Vector3)value;
            writer.WriteStartObject();
            writer.WritePropertyName("x");
            writer.WriteValue(vector3.x);
            writer.WritePropertyName("y");
            writer.WriteValue(vector3.y);
            writer.WritePropertyName("z");
            writer.WriteValue(vector3.z);
            writer.WriteEndObject();
        }
    }
}
