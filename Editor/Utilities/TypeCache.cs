using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace Treasured.UnitySdk.Utilities
{
    internal class TypeCache
    {
        static Dictionary<Type, Type> s_cachedEditorTypes = new Dictionary<Type, Type>();

        [InitializeOnLoadMethod]
        static void GetCustomEditorTypes()
        {
            /// TODO: Use types from UnityEditor.CustomEditorAttributes instead.
            FieldInfo fieldInfo = typeof(CustomEditor).GetField("m_InspectedType", BindingFlags.Instance | BindingFlags.NonPublic);
            var customEditorTypes = UnityEditor.TypeCache.GetTypesWithAttribute<CustomEditor>();
            foreach (var customEditorType in customEditorTypes)
            {
                var attr = customEditorType.GetCustomAttributes(false).FirstOrDefault();
                if (attr is CustomEditor customEditorAttribute)
                {
                    Type type = fieldInfo.GetValue(customEditorAttribute) as Type;
                    if (type != null)
                    {
                        s_cachedEditorTypes[type] = customEditorType;
                    }
                }
            }
        }

        /// <summary>
        /// Returns Custom Editor Type for the given type. They Custom Editor Type must inherit from <see cref="UnityEditor.Editor"/> and have <see cref="CustomEditor"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="customEditorType"></param>
        /// <returns></returns>
        public static bool TryGetCustomEditorType<T>(out Type customEditorType)
        {
            return s_cachedEditorTypes.TryGetValue(typeof(T), out customEditorType);
        }
    }
}
