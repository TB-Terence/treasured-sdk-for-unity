﻿using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Treasured.ExhibitXEditor
{
    [InitializeOnLoad]
    internal static class Styles
    {
        private static bool s_initialized;
        private static Texture2D _treasuredIcon;
        public static Texture2D TreasuredIcon
        {
            get
            {
                Init();
                return _treasuredIcon;
            }
        }

        public static GUIStyle Link { get; private set; }
        private static GUIStyle _dropZoneStyle;
        public static GUIStyle DropZone
        {
            get
            {
                _dropZoneStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
                return _dropZoneStyle;
            }
        }

        public static GUIStyle Label { get; }
        public static GUIStyle ShortLabel { get; }

        static Styles()
        {
            Label = new GUIStyle()
            {
                wordWrap = true,
                margin = new RectOffset(4, 4, 2, 2),
                padding = new RectOffset(2, 2, 1, 1),
                normal = { textColor = new Color(0.824f, 0.824f, 0.824f, 1) }
            };

            ShortLabel = new GUIStyle(Label)
            {
                fixedWidth = 64
            };

            Link = new GUIStyle(Label)
            {
                stretchWidth = false
            };

            Link.normal.textColor = new Color(0f, 0.47f, 0.85f);

            _dropZoneStyle = new GUIStyle()
            {
                margin = new RectOffset(3, 3, 3, 3),
                padding = new RectOffset(0, 0, 10, 10),
                fontStyle = FontStyle.Italic
            };
            DropZone.normal.textColor = Color.black;
            DropZone.alignment = TextAnchor.MiddleCenter;

            
        }

        static void Init()
        {
            if (s_initialized)
            {
                return;
            }
            _treasuredIcon = Resources.Load<Texture2D>("Treasured_Logo");
            s_initialized = true;
        }

        public static Texture2D CreateTexture2D(int width, int height, Color color)
        {
            Color[] colors = Enumerable.Repeat(color, width * height).ToArray();
            Texture2D texture = new Texture2D(width, height);
            texture.SetPixels(colors);
            texture.Apply();
            return texture;
        }
    }
}
