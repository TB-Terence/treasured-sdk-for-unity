using System;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Treasured.Actions;

namespace Treasured.UnitySdk
{
    public class ChoiceNodeConverter : JsonConverter<ChoiceNode>
    {
        public override ChoiceNode ReadJson(JsonReader reader, Type objectType, ChoiceNode existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return null;
        }

        public override void WriteJson(JsonWriter writer, ChoiceNode node, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("choices");
            writer.WriteStartArray();
            for (int i = 0; i < node.choices.Length; i++)
            {
                var port = node.GetOutputPort($"{nameof(ChoiceNode.choices)} {i}");
                if (port == null) continue;
                writer.WriteStartObject();
                writer.WritePropertyName("value");
                writer.WriteValue(node.choices[i]);
                writer.WritePropertyName("next");
                var newPort = node?.GetOutputPort(nameof(ChoiceNode.next));
                writer.WriteStartArray();
                if (port == null || port.Connection == null)
                {
                    writer.WriteEndArray();
                    writer.WriteEndObject();
                    continue;
                }
                ActionNode current = (ActionNode)port.Connection.node;
                while (current)
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("id");
                    writer.WriteValue(current.Id);
                    writer.WritePropertyName("method");
                    APIAttribute apiAttribute = GetType().GetCustomAttributes<APIAttribute>().FirstOrDefault();
                    string functionName = apiAttribute != null ? apiAttribute.FunctionName : current.Type;
                    writer.WriteValue(functionName);
                    writer.WritePropertyName("args");
                    serializer.Serialize(writer, current);
                    writer.WriteEndObject();
                    current = (ActionNode)current.GetOutputPort(nameof(ActionNode.next)).Connection?.node;
                }
                writer.WriteEndArray();
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }
    }
}
