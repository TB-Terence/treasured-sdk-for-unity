using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
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
            if (property.DeclaringType == typeof(TreasuredObject) && property.PropertyName == "visibleTargets")
            {
                property.ShouldSerialize = (instance) =>
                {
                    if (!(instance is TreasuredObject obj))
                    {
                        return false;
                    }
                    return obj.VisibleTargets is List<string> visibleTargets && visibleTargets.Count > 0;
                };
            }
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
                            case "displayMode":
                                return action.Type == "openLink" || action.Type == "playAudio" || action.Type == "playVideo";
                            case "content":
                                return action.Type == "showText";
                            case "targetId":
                                return action.Type == "selectObject";
                        }
                    }
                    return true;
                };
            }
            return property;
        }
    }
}
