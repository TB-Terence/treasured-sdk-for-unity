using System;
using Newtonsoft.Json;
namespace Treasured.UnitySdk
{
    public class InteractableButtonConverter : JsonConverter<InteractableButton>
    {
        public override bool CanRead => false;
        public override InteractableButton ReadJson(JsonReader reader, Type objectType, InteractableButton existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return null;
        }

        public override void WriteJson(JsonWriter writer, InteractableButton value, JsonSerializer serializer)
        {
            if (value.asset.IsNullOrNone())
            {
                writer.WriteNull();
            }
            else
            {
                writer.WriteStartObject();
                writer.WritePropertyName("icon");
                serializer.Serialize(writer, value.asset.name); // serialize only name, the actual svg data is under ./icons folder
                writer.WritePropertyName("transform");
                serializer.Serialize(writer, value.transform);
                writer.WriteEndObject();
            }
        }
    }
}
