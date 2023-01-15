using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Treasured.Actions;
using Treasured.Events;
using Treasured.UnitySdk;

namespace Treasured
{
    public class GuidedTourGraphConverter : JsonConverter<GuidedTour.GuidedTourGraph>
    {
        public override GuidedTour.GuidedTourGraph ReadJson(JsonReader reader, Type objectType, GuidedTour.GuidedTourGraph existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, GuidedTour.GuidedTourGraph graph, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            HashSet<string> titles = new HashSet<string>();
            foreach (var infoNode in graph.nodes.OfType<GuidedTour.GuidedTourInfoNode>())
            {
                if (titles.Contains(infoNode.title)) continue;
                titles.Add(infoNode.title);
                writer.WritePropertyName(infoNode.title);
                var port = infoNode?.GetOutputPort(nameof(GuidedTour.GuidedTourInfoNode.onTourStart));
                writer.WriteStartArray();
                if (port == null || port.Connection == null)
                {
                    writer.WriteEndArray();
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
            }
            writer.WriteEndObject();
        }
    }
}
