using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Treasured.UnitySdk
{
    public class ExporterConverter : JsonConverter<Exporter>
    {
        public override Exporter ReadJson(JsonReader reader, Type objectType, Exporter existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jtoken = JObject.ReadFrom(reader);
            string fullTypeName = jtoken["$type"].Value<string>();
            Type type = Type.GetType(fullTypeName);
            Exporter exporter = (Exporter)ScriptableObject.CreateInstance(type);
            serializer.Populate(jtoken.CreateReader(), exporter);
            return exporter;
        }

        public override void WriteJson(JsonWriter writer, Exporter value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}
