using System;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    internal static class Icons
    {
        public static GUIContent settings = EditorGUIUtility.TrIconContent("Settings", tooltip: "Settings");
        public static GUIContent editorSettings = EditorGUIUtility.TrIconContent("Settings", tooltip: "Editor Settings");
        public static GUIContent add = EditorGUIUtility.TrIconContent("Toolbar Plus", tooltip: "Create New");
        public static GUIContent remove = EditorGUIUtility.TrIconContent("Toolbar Minus", tooltip: "Remove Selected");

        public static readonly GUIContent menu = EditorGUIUtility.TrIconContent("_Menu");
        public static readonly GUIContent loop = EditorGUIUtility.TrIconContent("RotateTool On", "Enable if the hotspots should be looping during Guide Tour.");
        public static readonly GUIContent path = EditorGUIUtility.TrIconContent("NavMeshData Icon", "Show Path");
        public static readonly GUIContent folder = EditorGUIUtility.TrIconContent("Folder Icon");
        public static readonly GUIContent folderOpened = EditorGUIUtility.TrIconContent("FolderOpened Icon");
    }
}
