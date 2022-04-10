using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Treasured.UnitySdk
{
    public static class ReflectionUtilities
    {
        public struct SerializedFieldReference
        {
            public readonly object target;
            public readonly FieldInfo fieldInfo;
            public object Value
            {
                get
                {
                    return fieldInfo.GetValue(target);
                }
            }

            public SerializedFieldReference(object target, FieldInfo fieldInfo)
            {
                this.target = target;
                this.fieldInfo = fieldInfo;
            }
        }

        public static List<SerializedFieldReference> GetSeriliazedFieldReferences(object target)
        {
            List<SerializedFieldReference> references = new List<SerializedFieldReference>();
            var type = target.GetType();
            FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(x => x.IsPublic || x.IsDefined(typeof(UnityEngine.SerializeField))).ToArray();
            for (int i = 0; i < fieldInfos.Length; i++)
            {
                references.Add(new SerializedFieldReference(target, fieldInfos[i]));
                references.AddRange(GetSeriliazedFieldReferences(fieldInfos[i].GetValue(target)));
            }
            return references;
        }

        public static List<SerializedFieldReference> GetSeriliazedFieldReferencesWithAttribute<T>(object target) where T : Attribute
        {
            return GetSeriliazedFieldReferences(target).Where(reference => reference.fieldInfo.IsDefined(typeof(T))).ToList();
        }
    }
}
