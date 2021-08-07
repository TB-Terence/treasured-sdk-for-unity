using System;
using UnityEditor;
using UnityEngine;

namespace Treasured.ExhibitXEditor
{
    [InitializeOnLoad]
    public static class EditorGUIUtils
    {
        public static GUIContent IconContent(string iconName, string tooltip)
        {
            return IconContent(iconName, "", tooltip);
        }

        public static GUIContent IconContent(string iconName, string text, string tooltip)
        {
            return EditorGUIUtility.TrTextContent(text, tooltip, iconName);
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

        public static void CreateDropZone(Rect rect, Action onDrop)
        {
            DrawDottedBox(rect, EditorGUIUtility.isProSkin ? Color.white : Color.black, 2);
            Event e = Event.current;
            if (rect.Contains(e.mousePosition) && (e.type == EventType.DragUpdated || e.type == EventType.DragPerform))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (e.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    onDrop.Invoke();
                    Event.current.Use();
                }
            }
        }

        public static void DrawDottedBox(Rect rect, Color color, float space)
        {
            Handles.BeginGUI();
            Handles.color = color;
            Handles.DrawDottedLine(new Vector3(rect.xMin, rect.yMin), new Vector3(rect.xMax, rect.yMin), space * 4); // top
            Handles.DrawDottedLine(new Vector3(rect.xMin, rect.yMin), new Vector3(rect.xMin, rect.yMax), space); // left
            Handles.DrawDottedLine(new Vector3(rect.xMin, rect.yMax), new Vector3(rect.xMax, rect.yMax), space * 4); // bottom
            Handles.DrawDottedLine(new Vector3(rect.xMax, rect.yMin), new Vector3(rect.xMax, rect.yMax), space); // right
            Handles.color = Color.white;
            Handles.EndGUI();
        }
    }
}
