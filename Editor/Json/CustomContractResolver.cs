using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Reflection;
using UnityEngine;

namespace Treasured.UnitySdk.Editor
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
            if (property.DeclaringType == typeof(TreasuredAction))
            {
                property.ShouldSerialize = (instance) =>
                {
                    if (instance is TreasuredAction action)
                    {
                        switch (property.PropertyName)
                        {
                            case "id":
                            case "type":
                                return true;
                            case "src":
                                return action.Type == "openLink" || action.Type == "playAudio" || action.Type == "playVideo";
                            case "displayMode":
                                return action.Type == "openLink" || action.Type == "playVideo";
                            case "content":
                            case "style":
                                return action.Type == "showText";
                            case "targetId":
                                return action.Type == "selectObject";
                            case "volume":
                                return action.Type == "playAudio";
                        }
                    }
                    return true;
                };
            }
            return property;
        }
    }
}
