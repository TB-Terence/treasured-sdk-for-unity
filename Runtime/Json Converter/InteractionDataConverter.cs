using Newtonsoft.Json;
using System;
using System.Collections;
using UnityEngine;

namespace Treasured.ExhibitX
{
    public class InteractionDataConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(InteractionData);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            //Vector3 result = new Vector3();
            //serializer.Populate(reader, result);
            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }
            InteractionData interaction = value as InteractionData;
            writer.WriteStartObject();
            WritePropertyNameValue(writer, "type", value.GetType().Name);
            WritePropertyNameValue(writer, "position", interaction.position);
            switch (value)
            {
                case OpenLinkInteraction openLinkInteraction:
                    WritePropertyNameValue(writer, "url", openLinkInteraction.url);
                    WritePropertyNameValue(writer, "alignment", Enum.GetName(typeof(Alignment), openLinkInteraction.alignment));
                    break;
                case ShowTextInteraction showTextInteraction:
                    WritePropertyNameValue(writer, "content", showTextInteraction.content);
                    break;
                case PlayAudioInteraction playAudioInteraction:
                    WritePropertyNameValue(writer, "url", playAudioInteraction.url);
                    break;
                case LookAtInteraction lookAtInteraction:
                    WritePropertyNameValue(writer, "targetPosition", lookAtInteraction.targetPosition);
                    break;
            }
            writer.WriteEndObject();
        }

        private void WritePropertyNameValue(JsonWriter writer, string name, object value)
        {
            writer.WritePropertyName(name);
            if (value is Vector3 vector3)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("x");
                writer.WriteValue(vector3.x);
                writer.WritePropertyName("y");
                writer.WriteValue(vector3.y);
                writer.WritePropertyName("z");
                writer.WriteValue(vector3.z);
                writer.WriteEndObject();
            }
            else
            {
                writer.WriteValue(value);
            }
        }
    }
}
