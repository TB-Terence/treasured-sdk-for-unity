using System;
using System.Linq;
using System.Reflection;

namespace Treasured.UnitySdk
{
    public static class ReflectionUtilities
    {
        public static FieldInfo[] GetSeriliazedFields(object target)
        {
            Type type = target.GetType();
            FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return fieldInfos.Where(fieldInfo => fieldInfo.IsPublic || fieldInfo.IsDefined(typeof(UnityEngine.SerializeField))).ToArray();
        }

        public static FieldInfo[] GetSeriliazedFieldsWithAttribute<T>(object target) where T : Attribute
        {
            FieldInfo[] fieldInfos = GetSeriliazedFields(target);
            return fieldInfos.Where(fieldInfo => fieldInfo.IsDefined(typeof(T))).ToArray();
        }
    }
}
