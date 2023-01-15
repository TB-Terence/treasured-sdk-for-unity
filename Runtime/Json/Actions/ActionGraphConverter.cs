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
    public class ActionGraphConverter : JsonConverter<ActionGraph>
    {
        private static Dictionary<Type, Type[]> s_cachedEventNodes = new Dictionary<Type, Type[]>();
        public override ActionGraph ReadJson(JsonReader reader, Type objectType, ActionGraph existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return null;
        }

        public override void WriteJson(JsonWriter writer, ActionGraph graph, JsonSerializer serializer)
        {
            CacheCreateEventNodeAttributes(graph);
            writer.WriteStartObject();
            foreach (var kvp in s_cachedEventNodes)
            {
                if (kvp.Key != graph.Owner.GetType()) continue;
                foreach (var evt in kvp.Value)
                {
                    string eventName = evt.Name.Replace("Node", "");
                    writer.WritePropertyName(Char.ToLower(eventName[0]) + eventName.Substring(1));
                    var eventNode = graph.GetNodeOfType(evt);
                    var port = eventNode?.GetOutputPort(nameof(EventNode.action));
                    writer.WriteStartArray();
                    if(port == null || port.Connection == null)
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
            }
            writer.WriteEndObject();
        }

        void CacheCreateEventNodeAttributes(ActionGraph graph)
        {
            var attributes = ReflectionUtils.GetAttributes<CreateEventNodeAttribute>(graph.GetType());
            foreach(var attribute in attributes)
            {
                if (!s_cachedEventNodes.ContainsKey(attribute.OwnerType))
                {
                    s_cachedEventNodes.Add(attribute.OwnerType, attribute.Types);
                }
            }
        }
    }
}
