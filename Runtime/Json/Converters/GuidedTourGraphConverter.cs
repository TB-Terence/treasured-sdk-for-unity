using System;
using Newtonsoft.Json;

namespace Treasured.UnitySdk
{
    internal class GuidedTourGraphConverter : JsonConverter
    {
        public override bool CanRead => false;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(GuidedTourGraph);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is GuidedTourGraph guidedTourGraph)
            {
                writer.WriteStartObject();
                foreach (var tour in guidedTourGraph.tours)
                {
                    writer.WritePropertyName(tour.isDefault ? "default" : tour.title == "default" ? "untitled" : tour.title);
                    serializer.Serialize(writer, tour);
                }
                writer.WriteEndObject();
            }
        }
    }
}
