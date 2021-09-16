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

        private static readonly string[] _propertyNameOrder = new string[]
        {
            "id",
            "name"
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
            property.Order = GetOrder(property);
            if (property.DeclaringType.IsAssignableFrom(typeof(ScriptableObject)))
            {
                property.ShouldSerialize = (instance) =>
                {
                    if(property.PropertyName == "hideFlags")
                    {
                        return false;
                    }
                    if (property.PropertyName == "name" && !(instance is ObjectBase))
                    {
                        return false;
                    }
                    return true;
                };
            }
            return property;
        }

        //protected override JsonArrayContract CreateArrayContract(Type objectType)
        //{
        //    JsonArrayContract jsonArrayContract = base.CreateArrayContract(objectType);

        //    if (typeof(TreasuredObject).IsAssignableFrom(jsonArrayContract.CollectionItemType))
        //    {
        //        jsonArrayContract.ItemConverter = new TreasuredObjectConverter();
        //    }
        //    return jsonArrayContract;
        //}

        int GetOrder(JsonProperty property)
        {
            if (_propertyNameOrder.Contains(property.PropertyName))
            {
                return -999 + Array.IndexOf(_propertyNameOrder, property.PropertyName);
            }
            return property.Order == null ? 0 : (int)property.Order;
        }
    }
}
