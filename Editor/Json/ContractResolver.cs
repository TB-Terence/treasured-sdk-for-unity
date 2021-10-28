using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
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
            "created",
            "name",
            "type",
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
                contract.Converter = new StringEnumConverter(new KebabCaseNamingStrategy());
            }
            if (objectType == typeof(HotspotCamera))
            {
                contract.Converter = new HotspotCameraConverter();
            }
            if (objectType == typeof(Hitbox))
            {
                contract.Converter = new HitboxConverter();
            }
            if (objectType == typeof(Transform))
            {
                contract.Converter = new TransformConverter();
            }
            if (objectType == typeof(TreasuredObject) || objectType.GetElementType() == typeof(TreasuredObject) || (objectType.GenericTypeArguments.Length == 1 && objectType.GenericTypeArguments[0] == typeof(TreasuredObject)))
            {
                contract.Converter = new TreasuredObjectConverter();
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
            else if (type == typeof(ActionGroup))
            {
                properties = properties.Where(x => !x.PropertyName.Equals("name") && !x.PropertyName.Equals("hideFlags")).ToList();
            }
            return properties;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);
            property.Order = GetOrder(property); // Manually assign the order since we can't add JsonProperty(order) to the `name` field of UnityEngine.Object
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
