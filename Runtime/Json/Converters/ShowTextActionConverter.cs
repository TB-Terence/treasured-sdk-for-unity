using Newtonsoft.Json;
using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Treasured.UnitySdk
{
    internal class ShowTextActionConverter : JsonConverter
    {
        /// <summary>
        /// Words read per second. Calculation is based on average words read per minute / 60 seconds and round to integer.
        /// </summary>
        private int kAverageWordsReadPerSecond = 3;
        public override bool CanRead => false;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ShowTextAction);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is ShowTextAction action)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("id");
                serializer.Serialize(writer, action.Id);
                writer.WritePropertyName("type");
                serializer.Serialize(writer, action.Type);
                writer.WritePropertyName("content");
                serializer.Serialize(writer, action.Content);
                writer.WritePropertyName("style");
                serializer.Serialize(writer, action.Style);
                writer.WritePropertyName(nameof(action.duration));
                if (action.duration <= 0)
                {
                    string pattern = "[^\\w]";
                    serializer.Serialize(writer, Mathf.Max(1, Regex.Split(action.Content, pattern, RegexOptions.IgnoreCase).Length / kAverageWordsReadPerSecond));
                }
                else
                {
                    serializer.Serialize(writer, action.duration);
                }
                writer.WriteEndObject();
            }
        }
    }
}