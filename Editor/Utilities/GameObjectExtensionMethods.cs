using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    internal static class GameObjectExtensionMethods
    {
        private static MethodInfo setIconForObjectMethodInfo;
        private static MethodInfo getIconForObjectMethodInfo;

        public static readonly Texture2D dotGray = (Texture2D)EditorGUIUtility.IconContent("sv_icon_dot8_pix16_gizmo").image;


        private static void SetIconForObject(UnityEngine.Object obj, Texture2D icon)
        {
            if (setIconForObjectMethodInfo == null)
            {
                setIconForObjectMethodInfo = typeof(EditorGUIUtility).GetMethod("SetIconForObject", BindingFlags.Static | BindingFlags.NonPublic);
            }
            setIconForObjectMethodInfo?.Invoke(null, new object[] { obj, icon });
        }

        public static void SetLabelIcon(this GameObject go, int index)
        {
            index = Mathf.Clamp(index, 0, 7);
            SetIconForObject(go, (Texture2D)EditorGUIUtility.IconContent($"sv_label_{index}").image);
        }

        public static void SetDotIcon(this GameObject go, int index)
        {
            index = Mathf.Clamp(index, 0, 15);
            SetIconForObject(go, (Texture2D)EditorGUIUtility.IconContent($"sv_icon_dot{index}_pix16_gizmo").image);
        }

        public static Texture2D GetIcon(this GameObject go)
        {
            if (getIconForObjectMethodInfo == null)
            {
                getIconForObjectMethodInfo = typeof(EditorGUIUtility).GetMethod("GetIconForObject", BindingFlags.Static | BindingFlags.NonPublic);
            }
            return (Texture2D)(getIconForObjectMethodInfo?.Invoke(null, new object[] { go }));
        }
    }
}
