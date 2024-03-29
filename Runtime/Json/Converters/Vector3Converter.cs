﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using UnityEngine;

namespace Treasured.UnitySdk
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
            if(value is Vector3 vector3)
            {
                writer.WriteStartObject();
                writer.WritePropertyName(nameof(vector3.x));
                writer.WriteValue(vector3.x);
                writer.WritePropertyName(nameof(vector3.y));
                writer.WriteValue(vector3.y);
                writer.WritePropertyName(nameof(vector3.z));
                writer.WriteValue(vector3.z);
                writer.WriteEndObject();
            }
        }
    }
}
