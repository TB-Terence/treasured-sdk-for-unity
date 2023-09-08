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
        }

        protected override JsonContract CreateContract(Type objectType)
        {
            JsonContract contract = base.CreateContract(objectType);
            if (objectType == typeof(Vector3))
            {
                contract.Converter = new Vector3Converter();
            }
            if (objectType == typeof(string))
            {
                contract.Converter = new StringConverter();
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
            if (objectType == typeof(Quaternion))
            {
                contract.Converter = new QuaternionConverter();
            }
            if (objectType == typeof(Transform))
            {
                contract.Converter = new TransformConverter();
            }
            if (objectType == typeof(InteractableButton))
            {
                contract.Converter = new InteractableButtonConverter();
            }
            if (objectType == typeof(TreasuredObject) || objectType.GetElementType() == typeof(TreasuredObject) || (objectType.GenericTypeArguments.Length == 1 && objectType.GenericTypeArguments[0] == typeof(TreasuredObject)))
            {
                contract.Converter = new TreasuredObjectConverter();
            }
            if (objectType == typeof(Exporter))
            {
                contract.Converter = new ExporterConverter();
            }
            if (objectType == typeof(ActionGraph))
            {
                contract.Converter = new ActionGraphConverter();
            }
            if (objectType == typeof(GuidedTourGraph))
            {
                contract.Converter = new GuidedTourGraphConverter();
            }
            if (objectType == typeof(ActionCollection))
            {
                contract.Converter = new ActionCollectionConverter();
            }
            if (objectType == typeof(VideoInfo) || objectType == typeof(AudioInfo) || objectType == typeof(ImageInfo))
            {
                contract.Converter = new MediaInfoConverter();
            }
            if (objectType == typeof(PlayAudioAction))
            {
                contract.Converter = new PlayAudioActionConverter();
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
            else if (type == typeof(GuidedTourGraph) || type == typeof(GuidedTour) || type == typeof(ActionGroup) || typeof(Exporter).IsAssignableFrom(type) || typeof(TreasuredSDKPreferences).IsAssignableFrom(type))
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
            //if (typeof(ScriptableActionCollection).IsAssignableFrom(type))
            //{
            //    property.ValueProvider = new ScriptableObjectValueProvider(CreateMemberValueProvider(member), type);
            //}
            if (property.PropertyType == typeof(CustomHTML))
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
