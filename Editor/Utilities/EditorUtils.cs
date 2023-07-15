using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Treasured.UnitySdk.Utilities
{
    internal sealed class EditorUtils
    {
        private static MethodInfo openPropertyEditorMethodInfo;

        public static void OpenPropertyEditor(UnityEngine.Object obj)
        {
#if UNITY_2020_1_OR_NEWER
            if (openPropertyEditorMethodInfo == null)
            {
                Type propertyEditorType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.PropertyEditor");
                if (propertyEditorType != null)
                {
                    openPropertyEditorMethodInfo = propertyEditorType.GetMethod("OpenPropertyEditor", BindingFlags.Static | BindingFlags.NonPublic);
                }
            }
            openPropertyEditorMethodInfo?.Invoke(null, new object[] { obj, true });
#endif
        }
        
        /// <summary>
        /// Gets the object that the property is a member of
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public static object GetTargetObjectWithProperty(SerializedProperty property)
        {
            string path = property.propertyPath.Replace(".Array.data[", "[");
            object obj = property.serializedObject.targetObject;
            string[] elements = path.Split('.');

            for (int i = 0; i < elements.Length - 1; i++)
            {
                string element = elements[i];
                if (element.Contains("["))
                {
                    string elementName = element.Substring(0, element.IndexOf("["));
                    int index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetValue_Imp(obj, elementName, index);
                }
                else
                {
                    obj = GetValue_Imp(obj, element);
                }
            }

            return obj;
        }
        
        /// <summary>
        /// Gets the object the property represents.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public static object GetTargetObjectOfProperty(SerializedProperty property)
        {
            if (property == null)
            {
                return null;
            }

            string path = property.propertyPath.Replace(".Array.data[", "[");
            object obj = property.serializedObject.targetObject;
            string[] elements = path.Split('.');

            foreach (var element in elements)
            {
                if (element.Contains("["))
                {
                    string elementName = element.Substring(0, element.IndexOf("["));
                    int index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetValue_Imp(obj, elementName, index);
                }
                else
                {
                    obj = GetValue_Imp(obj, element);
                }
            }

            return obj;
        }
        
        internal static object GetValue_Imp(object source, string name)
        {
            if (source == null)
            {
                return null;
            }

            Type type = source.GetType();

            while (type != null)
            {
                FieldInfo field = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (field != null)
                {
                    return field.GetValue(source);
                }

                PropertyInfo property = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (property != null)
                {
                    return property.GetValue(source, null);
                }

                type = type.BaseType;
            }

            return null;
        }

        internal static object GetValue_Imp(object source, string name, int index)
        {
            IEnumerable enumerable = GetValue_Imp(source, name) as IEnumerable;
            if (enumerable == null)
            {
                return null;
            }

            IEnumerator enumerator = enumerable.GetEnumerator();
            for (int i = 0; i <= index; i++)
            {
                if (!enumerator.MoveNext())
                {
                    return null;
                }
            }

            return enumerator.Current;
        }
        
        public static MethodInfo GetMethod(object target, string methodName)
        {
            return GetAllMethods(target, m => m.Name.Equals(methodName, StringComparison.Ordinal)).FirstOrDefault();
        }
        
        public static IEnumerable<MethodInfo> GetAllMethods(object target, Func<MethodInfo, bool> predicate)
        {
            if (target == null)
            {
                Debug.LogError("The target object is null. Check for missing scripts.");
                yield break;
            }

            List<Type> types = GetSelfAndBaseTypes(target);

            for (int i = types.Count - 1; i >= 0; i--)
            {
                IEnumerable<MethodInfo> methodInfos = types[i]
                    .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    .Where(predicate);

                foreach (var methodInfo in methodInfos)
                {
                    yield return methodInfo;
                }
            }
        }
        
        /// <summary>
        ///		Get type and all base types of target, sorted as following:
        ///		<para />[target's type, base type, base's base type, ...]
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        private static List<Type> GetSelfAndBaseTypes(object target)
        {
            List<Type> types = new List<Type>()
            {
                target.GetType()
            };

            while (types.Last().BaseType != null)
            {
                types.Add(types.Last().BaseType);
            }

            return types;
        }

        internal static void PreviewCamera(HotspotCamera hotspotCamera)
        {
            SceneView.lastActiveSceneView.LookAt(hotspotCamera.transform.position, hotspotCamera.transform.rotation, 0.01f);
        }

        public static void Focus(int amount, params Transform[] targets)
        {
            Camera sceneCamera = SceneView.lastActiveSceneView.camera;
            if (sceneCamera != null)
            {
                if (targets.Length > 1)
                {
                    // Calcuate average position from all hotspots
                    Vector3 center = Vector3.zero;
                    foreach (Transform target in targets)
                    {
                        center += target.transform.position;
                    }
                    center /= targets.Length;
                    SceneView.lastActiveSceneView.LookAt(center, Quaternion.Euler(45f, 45f, 0), amount);
                }
                else if(targets.Length == 1)
                {
                    // Set the camera position and rotation for an isometric view
                    SceneView.lastActiveSceneView.LookAt(targets[0].transform.position, Quaternion.Euler(45f, 45f, 0), amount);
                }
            }
        }
    }
    
    
}
