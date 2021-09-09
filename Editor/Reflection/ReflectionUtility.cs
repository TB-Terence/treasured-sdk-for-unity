using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk.Editor
{
    [InitializeOnLoad]
    public static class ReflectionUtility
    {
        private static readonly Type PropertyEditor;
        private static readonly MethodInfo OpenPropertyEditorMethod;

        static ReflectionUtility()
        {
            PropertyEditor = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.PropertyEditor");
            if (PropertyEditor != null)
            {
                OpenPropertyEditorMethod = PropertyEditor.GetMethod("OpenPropertyEditor", BindingFlags.Static | BindingFlags.NonPublic);
            }
        }

        public static void OpenPropertyEditor(UnityEngine.Object obj)
        {
#if UNITY_2020_1_OR_NEWER
            if (OpenPropertyEditorMethod == null)
            {
                Debug.LogError("Failed to open property editor. This is likely caused by Unity changed the scripts internally.");
            }
            else
            {
                OpenPropertyEditorMethod.Invoke(null, new object[] { obj, true });
            }
#endif
        }
    }
}
