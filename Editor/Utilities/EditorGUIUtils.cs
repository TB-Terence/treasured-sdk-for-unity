using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace Treasured.UnitySdk.Utilities
{
    [InitializeOnLoad]
    internal sealed class EditorGUIUtils
    {

        /// <summary>
        /// Filter for Icons.
        /// Finds all asset's file name contains Fa and is type texture2D.
        /// </summary>
        private const string kButtonIconFilter = "Fa t:texture2D";
        private static readonly string[] s_buttonIconFolderPaths = new string[] { "Packages/com.treasured.unitysdk/Resources/Icons/Objects/Font Awesome" };
        public static GUIContent[] buttonIcons;
        public static GUIContent DefaultButtonIcon
        {
            get; private set;
        }

        public static Texture2D DefaultButtonIconTexture
        {
            get
            {
                return DefaultButtonIcon != null ? (Texture2D)DefaultButtonIcon.image : null;
            }
        }

        static EditorGUIUtils()
        {
            LoadIcons();
        }

        private static void LoadIcons()
        {
            var guids = AssetDatabase.FindAssets(kButtonIconFilter, s_buttonIconFolderPaths);
            if (buttonIcons is null || guids.Length != buttonIcons.Length)
            {
                buttonIcons = new GUIContent[guids.Length];
                for (int i = 0; i < guids.Length; i++)
                {
                    string guid = guids[i];
                    string texturePath = AssetDatabase.GUIDToAssetPath(guid);
                    Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                    buttonIcons[i] = new GUIContent(string.Empty, texture, texture.name);
                }
                if (TryGetButtonIcon("FaCircle", out var button))
                {
                    DefaultButtonIcon = button;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="icon"></param>
        /// <returns>True if the name of the icon is found.</returns>
        public static bool TryGetButtonIcon(string name, out GUIContent icon)
        {
            icon = buttonIcons.FirstOrDefault(icon => icon.tooltip.Equals(name));
            return icon != null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="texture"></param>
        /// <returns>True if the name of the icon is found.</returns>
        public static bool TryGetButtonIconTexture(string name, out Texture2D texture)
        {
            texture = null;
            if (TryGetButtonIcon(name, out var guiContent))
            {
                texture = (Texture2D)guiContent.image;
            }
            return texture != null;
        }

        public static void DrawPropertiesExcluding(SerializedObject serializedObject, params string[] propertyToExclude)
        {
            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (!propertyToExclude.Contains(iterator.name))
                {
                    EditorGUILayout.PropertyField(iterator, true);
                }
            }
        }
    }
}
