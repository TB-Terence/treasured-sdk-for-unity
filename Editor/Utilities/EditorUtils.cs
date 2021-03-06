using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

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
    }
}
