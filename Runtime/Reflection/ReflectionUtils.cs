using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Treasured.UnitySdk
{
    public static class ReflectionUtils
    {
        public struct SerializedFieldReference
        {
            public readonly object target;
            public readonly FieldInfo fieldInfo;

            public SerializedFieldReference(object target, FieldInfo fieldInfo)
            {
                this.target = target;
                this.fieldInfo = fieldInfo;
            }

            public object GetValue()
            {
                return fieldInfo.GetValue(target);
            }

            public void SetValue(object value)
            {
                fieldInfo.SetValue(target, value);
            }

            /// <summary>
            /// </summary>
            /// <returns>
            /// <para>If <see cref="Value"/> is <see cref="UnityEngine.Object"/>, return <see cref="UnityEngine.Object.GetInstanceID"/> == 0.</para>
            /// <para>If <see cref="Value"/> is <see cref="System.string"/>, return <see cref="string.IsNullOrWhiteSpace(string)"/>.</para>
            /// <para>Otherwise return <see cref="Value"/> is null.</para>
            /// </returns>
            public bool IsNull()
            {
                var value = GetValue();
                if (value is UnityEngine.Object obj && obj.GetInstanceID() == 0)
                {
                    return true;
                }
                if (value is string str)
                {
                    return string.IsNullOrWhiteSpace(str);
                }
                return value is null;
            }
        }

        /// <summary> 
        /// </summary>
        /// <param name="target"></param>
        /// <returns>
        /// All serialized fields of <paramref name="target"/>. This includes all nested fields that are public or with <see cref="UnityEngine.SerializeField"/> attribute.</returns>
        public static List<SerializedFieldReference> GetSeriliazedFieldReferences(object target)
        {
            List<SerializedFieldReference> list = new List<SerializedFieldReference>();
            if (target == null)
            {
                return list;
            }
            var type = target.GetType();
            FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(x => x.IsPublic || x.IsDefined(typeof(UnityEngine.SerializeField))).ToArray();
            for (int i = 0; i < fieldInfos.Length; i++)
            {
                list.Add(new SerializedFieldReference(target, fieldInfos[i]));
                list.AddRange(GetSeriliazedFieldReferences(fieldInfos[i].GetValue(target)));
            }
            return list;
        }

        /// <summary>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <returns>All serialized fields with attribute <typeparamref name="T"/> of <paramref name="target"/>. This includes all nested fields that are public or with <see cref="UnityEngine.SerializeField"/> attribute.</returns>
        public static List<SerializedFieldReference> GetSeriliazedFieldReferencesWithAttribute<T>(object target) where T : Attribute
        {
            return GetSeriliazedFieldReferences(target).Where(reference => reference.fieldInfo.IsDefined(typeof(T))).ToList();
        }

        public static T[] GetFieldValuesOfType<T>(object target, BindingFlags bindingFlags)
        {
            Type type = target.GetType();
            FieldInfo[] fieldInfos = type.GetFields(bindingFlags);
            List<T> results = new List<T>();
            foreach (var fi in fieldInfos)
            {
                if (!typeof(T).IsAssignableFrom(fi.FieldType))
                {
                    continue;
                }
                var value = fi.GetValue(target);
                if (value == null)
                {
                    continue;
                }
                results.Add((T)value);
            }
            return results.ToArray();
        }

        public static T[] GetSerializedFieldValuesOfType<T>(object target)
        {
            Type type = target.GetType();
            FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(x => x.IsPublic || x.IsDefined(typeof(SerializeField))).ToArray();
            List<T> results = new List<T>();
            foreach (var fi in fieldInfos)
            {
                if (!typeof(T).IsAssignableFrom(fi.FieldType))
                {
                    continue;
                }
                var value = fi.GetValue(target);
                if (value == null)
                {
                    continue;
                }
                results.Add((T)value);
            }
            return results.ToArray();
        }
    }
}
