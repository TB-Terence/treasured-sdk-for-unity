using UnityEngine;
using UnityEditor;

namespace Treasured.UnitySdk
{
    internal static class EditorGUIUtils
    {
        static class Styles
        {
            public static GUIStyle Link = new GUIStyle() { stretchWidth = false, normal = { textColor = new Color(0f, 0.47f, 0.85f) } };
        }

        public static bool Link(GUIContent label, GUIContent url)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);
            Rect position = GUILayoutUtility.GetRect(url, Styles.Link);
            Handles.BeginGUI();
            Handles.color = Styles.Link.normal.textColor;
            Handles.DrawLine(new Vector3(position.xMin, position.yMax), new Vector3(position.xMax, position.yMax));
            Handles.color = Color.white;
            Handles.EndGUI();
            EditorGUIUtility.AddCursorRect(position, MouseCursor.Link);
            EditorGUILayout.EndHorizontal();
            return GUI.Button(position, url, Styles.Link);
        }

        public static Rect OffsetRight(this Rect rect, int width, int height)
        {
            return new Rect(rect.xMax - width, rect.y, width, width);
        }
    }
}
