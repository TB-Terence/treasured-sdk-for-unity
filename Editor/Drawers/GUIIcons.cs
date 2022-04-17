using System;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    internal static class GUIIcons
    {
        public static GUIContent settings = EditorGUIUtility.TrIconContent("Settings", tooltip: "Settings");
        public static GUIContent add = EditorGUIUtility.TrIconContent("Toolbar Plus", tooltip: "Create New");
        public static GUIContent remove = EditorGUIUtility.TrIconContent("Toolbar Minus", tooltip: "Remove Selected");
        public static GUIContent warning = EditorGUIUtility.TrIconContent("Warning");

        public static readonly GUIContent menu = EditorGUIUtility.TrIconContent("_Menu");
        public static readonly GUIContent folder = EditorGUIUtility.TrIconContent("Folder Icon");
        public static readonly GUIContent folderOpened = EditorGUIUtility.TrIconContent("FolderOpened Icon");
    }
}
