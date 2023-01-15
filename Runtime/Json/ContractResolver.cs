using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Treasured.Actions;
using UnityEngine;

namespace Treasured.UnitySdk
{
    internal class ContractResolver : DefaultContractResolver
    {
        public static readonly ContractResolver Instance = new ContractResolver();
        private static JsonConverter[] s_customJsonConverters;

        private static readonly string[] propertyNameOrder = new string[]
        {
            "version",
            "id",
            "created",
            "name",
            "type",
            "function",
            "author",
            "title",
            "description",
            "loader",
            "hitbox",
            "camera"
        };

        public ContractResolver()
        {
            NamingStrategy = new CamelCaseNamingStrategy
            {
                ProcessDictionaryKeys = true,
                OverrideSpecifiedNames = true
            };
            s_customJsonConverters = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsClass && !t.IsAbstract && typeof(JsonConverter).IsAssignableFrom(t)).Select(t => (JsonConverter)Activator.CreateInstance(t)).ToArray();
        }

        protected override JsonContract CreateContract(Type objectType)
        {
            JsonContract contract = base.CreateContract(objectType);
            foreach (var converter in s_customJsonConverters)
            {
                if (converter.CanConvert(objectType))
                {
                    contract.Converter = converter;
                    break;
                }
            }
            return contract;
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var properties = base.CreateProperties(type, memberSerialization);
            if (type.IsSubclassOf(typeof(MonoBehaviour)))
            {
                // filter out `name` field if type is subclass of TreasuredObject OR if DeclaringType of the property is subclass of MonoBehaviour
                properties = properties.Where(x => (x.PropertyName.Equals("name") && type.IsSubclassOf(typeof(TreasuredObject))) || x.DeclaringType.IsSubclassOf(typeof(MonoBehaviour))).ToList();
            }
            else if (typeof(ActionNode).IsAssignableFrom(type))
            {
                properties = properties.Where(x => x.DeclaringType == type).ToList();
            }
            else if (type == typeof(ScriptableActionCollection) || type == typeof(GuidedTourGraph) || type == typeof(GuidedTour) || type == typeof(ActionGroup) || typeof(Exporter).IsAssignableFrom(type) || typeof(TreasuredSDKPreferences).IsAssignableFrom(type))
            {
                properties = properties.Where(x => !x.PropertyName.Equals("name") && !x.PropertyName.Equals("hideFlags")).ToList();
            }
            else if (typeof(ScriptableObject).IsAssignableFrom(type))
            {
                properties = properties.Where(x => !x.PropertyName.Equals("hideFlags")).ToList();
            }
            return properties;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);
            property.Order = GetOrder(property); // Manually assign the order since we can't add JsonProperty(order) to the `name` field of UnityEngine.Object
            Type type = property.PropertyType;
            if (typeof(ScriptableActionCollection).IsAssignableFrom(type))
            {
                property.ValueProvider = new ScriptableObjectValueProvider(CreateMemberValueProvider(member), type);
            }
            else if (property.PropertyType == typeof(CustomEmbed))
            {
                property.ValueProvider = new ObjectValueProvider(CreateMemberValueProvider(member), type);
            }
            return property;
        }

        int GetOrder(JsonProperty property)
        {
            if (propertyNameOrder.Contains(property.PropertyName))
            {
                return -999 + Array.IndexOf(propertyNameOrder, property.PropertyName);
            }
            return property.Order == null ? 0 : (int)property.Order;
        }

        class ScriptableObjectValueProvider : IValueProvider
        {
            private IValueProvider innerProvider;
            private object defaultValue;

            public ScriptableObjectValueProvider(IValueProvider innerProvider, Type type)
            {
                this.innerProvider = innerProvider;
                defaultValue = ScriptableObject.CreateInstance(type);
            }

            public void SetValue(object target, object value)
            {
                innerProvider.SetValue(target, value ?? defaultValue);
            }

            public object GetValue(object target)
            {
                return innerProvider.GetValue(target) ?? defaultValue;
            }
        }

        class ObjectValueProvider : IValueProvider
        {
            private IValueProvider innerProvider;
            private object defaultValue;

            public ObjectValueProvider(IValueProvider innerProvider, Type type)
            {
                this.innerProvider = innerProvider;
                defaultValue = Activator.CreateInstance(type);
            }

            public void SetValue(object target, object value)
            {
                innerProvider.SetValue(target, value ?? defaultValue);
            }

            public object GetValue(object target)
            {
                return innerProvider.GetValue(target) ?? defaultValue;
            }
        }

    }
}
