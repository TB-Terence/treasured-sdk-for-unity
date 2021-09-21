using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Treasured.UnitySdk
{
    internal class ContractResolver : DefaultContractResolver
    {
        public static readonly ContractResolver Instance = new ContractResolver();

        private static readonly string[] propertyNameOrder = new string[]
        {
            "version",
            "id",
            "name",
            "type"
        };

        public ContractResolver()
        {
            NamingStrategy = new CamelCaseNamingStrategy
            {
                ProcessDictionaryKeys = true,
                OverrideSpecifiedNames = true
            };
        }

        protected override JsonContract CreateContract(Type objectType)
        {
            JsonContract contract = base.CreateContract(objectType);
            if (objectType == typeof(Vector3))
            {
                contract.Converter = new Vector3Converter();
            }
            if (objectType.BaseType == typeof(Enum))
            {
                contract.Converter = new StringEnumConverter(new CamelCaseNamingStrategy());
            }
            return contract;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);
            property.Order = GetOrder(property); // Manually assign the order since we can't add JsonProperty(order) to the `name` field of UnityEngine.Object
            property.ShouldSerialize = (instance) =>
            {
                if (property.PropertyName.Equals("name"))
                {
                    return !property.DeclaringType.IsAssignableFrom(typeof(TreasuredMap));
                }
                // Only allow the `name` field of the UnityEngine.Object to be serialized, ignore all properties that are assignable from MonoBehaviour(e.g. MonoBehaviour, Behaviour, Component, and UnityEngine.Object)
                return !property.DeclaringType.IsAssignableFrom(typeof(MonoBehaviour));
            };
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
    }
}
