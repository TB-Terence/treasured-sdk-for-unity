using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Reflection;
using UnityEngine;

namespace Treasured.SDK
{
    internal class CustomContractResolver : DefaultContractResolver
    {
        public static readonly CustomContractResolver Instance = new CustomContractResolver();

        public CustomContractResolver()
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

            // this will only be called once for each objectType and then cached
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

            // ignore name and hideFlags for ScriptableObject
            if (property.DeclaringType.IsAssignableFrom(typeof(ScriptableObject)) && (property.PropertyName == "name" || property.PropertyName == "hideFlags"))
            {
                property.ShouldSerialize = (instance) => false;
            }

            return property;
        }
    }
}
