using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    internal static class ExtensionMethods
    {
        public static string GetFullPath(this Component component)
        {
            string path = component.gameObject.transform.name;
            if (component.gameObject.transform.parent == null)
            {
                return path;
            }
            return $"{GetFullPath(component.gameObject.transform.parent)}/{path}";
        }

        public static string GetRelativePath<T>(this Component component) where T : Component
        {
            string path = component.gameObject.transform.name;
            if (component.gameObject.transform.parent.GetComponent<T>())
            {
                return path;
            }
            return $"{GetRelativePath<T>(component.gameObject.transform.parent)}/{path}";
        }

        /// <summary>
        /// Invoke all method with the given name. The methods in the base class will be invoked first.
        /// </summary>
        /// <param name="monoBehaviour"></param>
        /// <param name="name"></param>
        /// <param name="args"></param>
        public static void TryInvokeMethods(this MonoBehaviour monoBehaviour, string name, object[] args = null)
        {
            Type type = monoBehaviour.GetType();
            Stack<MethodInfo> methods = new Stack<MethodInfo>();
            while (type != null)
            {
                MethodInfo method = type.GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (method != null)
                {
                    methods.Push(method);
                }
                type = type.BaseType == typeof(MonoBehaviour) ? null : type.BaseType;
            }
            foreach (var mi in methods)
            {
                mi.Invoke(monoBehaviour, args);
            }
#if UNITY_EDITOR
            EditorUtility.SetDirty(monoBehaviour);
#endif
        }
    }
}
