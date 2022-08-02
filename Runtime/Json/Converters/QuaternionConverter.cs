using Newtonsoft.Json;
using System;
using UnityEngine;
using UnityGLTF.Extensions;

namespace Treasured.UnitySdk
{
    public class QuaternionConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Quaternion);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if(value is Quaternion quaternion)
            {
                GLTF.Math.Quaternion temp = quaternion.ToGltfQuaternionConvert();
                serializer.Serialize(writer, temp);
            }
        }
    }
}
