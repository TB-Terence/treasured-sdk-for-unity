using System;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    internal static class EditorGUILayoutUtilities
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

        public static void CreateDropZone(Rect rect, DragAndDropVisualMode visualMode, Action<UnityEngine.Object[]> onDrop)
        {
            //DrawDottedBox(rect, EditorGUIUtility.isProSkin ? Color.white : Color.black, 2);
            //GUI.Box(rect, label, CustomStyles.DropZone);
            Event e = Event.current;
            if (rect.Contains(e.mousePosition) && (e.type == EventType.DragUpdated || e.type == EventType.DragPerform))
            {
                DragAndDrop.visualMode = visualMode;
                if (e.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    onDrop.Invoke(DragAndDrop.objectReferences);
                    Event.current.Use();
                }
            }
        }

        public static bool CreateClickZone(Event e, Rect rect, MouseCursor mouseCursor, int button)
        {
            EditorGUIUtility.AddCursorRect(rect, mouseCursor);
            bool clicked = rect.Contains(e.mousePosition) && (e.type == EventType.MouseDown || e.type == EventType.MouseUp) && e.button == button;
            if (clicked)
            {
                e.Use();
            }
            return clicked;
        }

        public static bool CreateClickZone(Event e, Rect rect, int button)
        {
            bool clicked = rect.Contains(e.mousePosition) && (e.type == EventType.MouseDown || e.type == EventType.MouseUp) && e.button == button;
            if (clicked)
            {
                e.Use();
            }
            return clicked;
        }
    }
}
