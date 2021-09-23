using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk.Editor
{
    internal static class DefaultStyles
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
        public static GUIStyle TabBar { get; private set; }
        public static GUIStyle TabButton { get; private set; }
        public static GUIStyle TabPage { get; private set; }
        public static GUIStyle IndexLabel { get; private set; }

        public static GUIStyle Link { get; private set; }
        public static GUIStyle DropZone { get; private set; }

        public static GUIStyle Label { get; private set; }
        public static GUIStyle ShortLabel { get; private set; }
        public static GUIStyle ToolbarButton { get; private set; }
        public static GUIStyle Panel { get; private set; }

        public static void Init()
        {
            if (s_initialized)
            {
                return;
            }
            _treasuredIcon = Resources.Load<Texture2D>("Treasured_Logo");
            Label = new GUIStyle()
            {
                wordWrap = true,
                margin = new RectOffset(4, 4, 2, 2),
                padding = new RectOffset(2, 2, 1, 1),
                normal = { textColor = new Color(0.824f, 0.824f, 0.824f, 1) }
            };

            IndexLabel = new GUIStyle("label")
            {
                alignment = TextAnchor.MiddleRight
            };

            TabBar = new GUIStyle()
            {
                margin = new RectOffset(4, 4, 0, 0)
            };

            Texture2D lightGrey = CreateTexture2D(1, 1, new Color(Color.gray.r, Color.gray.g, Color.gray.b, 0.15f));

            TabButton = new GUIStyle(EditorStyles.toolbarButton)
            {
                fixedHeight = 24,
                fontStyle = FontStyle.Bold,
                overflow = new RectOffset(0, 0, 0, 1),
                normal =
                {
                    background = Texture2D.blackTexture
                },
                onNormal = 
                {
                    background = lightGrey
                }
            };

            TabPage = new GUIStyle("box")
            {
                margin = new RectOffset(4, 4, 0, 4),
                normal =
                {
                    background = lightGrey
                }
            };

            ToolbarButton = new GUIStyle(Label);

            ShortLabel = new GUIStyle(Label)
            {
                fixedWidth = 64
            };

            Link = new GUIStyle(Label)
            {
                stretchWidth = false
            };

            Link.normal.textColor = new Color(0f, 0.47f, 0.85f);

            DropZone = new GUIStyle()
            {
                margin = new RectOffset(3, 3, 3, 3),
                padding = new RectOffset(0, 0, 10, 10),
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.grey },
                alignment = TextAnchor.MiddleCenter
            };
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
