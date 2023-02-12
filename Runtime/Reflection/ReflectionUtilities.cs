using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Treasured.UnitySdk
{
    public struct FieldInfoValuePair
    {
        public object DeclaringObject { get; private set; }
        public FieldInfo FieldInfo { get; private set; }

        public FieldInfoValuePair(object declaringObject, FieldInfo fieldInfo)
        {
            DeclaringObject = declaringObject;
            FieldInfo = fieldInfo;
        }

        public void SetValue(object value)
        {
            FieldInfo.SetValue(DeclaringObject, value);
        }

        public object GetValue()
        {
            return FieldInfo.GetValue(DeclaringObject);
        }

        public T GetValueAs<T>()
        {
            return (T)FieldInfo.GetValue(DeclaringObject);
        }

        public bool IsNull()
        {
            var value = GetValue();
            if (FieldInfo.FieldType.IsAssignableFrom(typeof(UnityEngine.Object)))
            {
                return (value as UnityEngine.Object).IsNullOrNone();
            }
            if (value is string str)
            {
                return string.IsNullOrWhiteSpace(str);
            }
            return value is null;
        }
    }

    public static class ReflectionUtilities
    {
        public static IEnumerable<FieldInfoValuePair> GetSerializableFieldValuesOfType<T>(object target, bool includeChildren = false)
        {
            List<FieldInfoValuePair> fieldValuePairs = new List<FieldInfoValuePair>();
            foreach (var pair in GetSerializableFieldInfoValuePair(target, includeChildren))
            {
                if (typeof(T).IsAssignableFrom(pair.FieldInfo.FieldType))
                {
                    fieldValuePairs.Add(new FieldInfoValuePair(target, pair.FieldInfo));
                }
            }
            return fieldValuePairs;
        }

        /// <summary>
        /// Gets all serializable fields. This includes all fields that are public or with <see cref="UnityEngine.SerializeField"/> attribute.
        /// </summary>
        /// <param name="obj">The target object to get the fields</param>
        /// <param name="includeChildren">If it should include nested fields</param>
        /// <returns></returns>
        public static IEnumerable<FieldInfoValuePair> GetSerializableFieldInfoValuePair(object obj, bool includeChildren = false)
        {
            List<FieldInfoValuePair> fieldValuePairs = new List<FieldInfoValuePair>();
            Type type = obj.GetType();
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (FieldInfo field in fields)
            {
                if (field.IsPublic || Attribute.IsDefined(field, typeof(SerializeField)))
                {
                    fieldValuePairs.Add(new FieldInfoValuePair(obj, field));
                }
            }
            if (includeChildren)
            {
                List<FieldInfoValuePair> childFieldValuePairs = new List<FieldInfoValuePair>();
                foreach (FieldInfoValuePair fieldValuePair in fieldValuePairs)
                {
                    if (fieldValuePair.FieldInfo.FieldType.IsClass)
                    {
                        object childObj = fieldValuePair.GetValue();
                        childFieldValuePairs.AddRange(GetSerializableFieldInfoValuePair(childObj, includeChildren));
                    }
                }
                fieldValuePairs.AddRange(childFieldValuePairs);
            }
            return fieldValuePairs;
        }

        /// <summary>
        /// Gets all serializable fields with attribute <typeparamref name="T"/>. This includes all fields that are public or with <see cref="UnityEngine.SerializeField"/> attribute.
        /// </summary>
        /// <typeparam name="T">Attribute</typeparam>
        /// <param name="obj">The target object to get the fields</param>
        /// <param name="includeChildren">If it should include nested fields</param>
        /// <returns></returns>
        public static IEnumerable<FieldInfoValuePair> GetSerializableFieldInfoValuePairWithAttribute<T>(object obj, bool includeChildren = false) where T : Attribute
        {
            return GetSerializableFieldInfoValuePair(obj, includeChildren).Where(pair => Attribute.IsDefined(pair.FieldInfo, typeof(T))).ToList();
        }
    }
}
