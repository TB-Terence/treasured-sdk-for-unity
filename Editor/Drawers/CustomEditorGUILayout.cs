using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Treasured.SDKEditor
{
    [InitializeOnLoad]
    internal static class CustomEditorGUILayout
    {
        private static readonly DirectoryInfo DataDirectoryInfo = new DirectoryInfo(Application.dataPath);

        public static bool Link(GUIContent label, GUIContent url)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);
            Rect position = GUILayoutUtility.GetRect(url, CustomStyles.Link);

            Handles.BeginGUI();
            Handles.color = CustomStyles.Link.normal.textColor;
            Handles.DrawLine(new Vector3(position.xMin, position.yMax), new Vector3(position.xMax, position.yMax));
            Handles.color = Color.white;
            Handles.EndGUI();

            EditorGUIUtility.AddCursorRect(position, MouseCursor.Link);
            EditorGUILayout.EndHorizontal();
            return GUI.Button(position, url, CustomStyles.Link);
        }

        public static void CreateDropZone(Rect rect, GUIContent label, Action<UnityEngine.Object[]> onDrop)
        {
            DrawDottedBox(rect, EditorGUIUtility.isProSkin ? Color.white : Color.black, 2);
            GUI.Box(rect, label, CustomStyles.DropZone);
            Event e = Event.current;
            if (rect.Contains(e.mousePosition) && (e.type == EventType.DragUpdated || e.type == EventType.DragPerform))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (e.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    onDrop.Invoke(DragAndDrop.objectReferences);
                    Event.current.Use();
                }
            }
        }

        public static void DrawDottedBox(Rect rect, Color color, float space)
        {
            Handles.color = color;
            Handles.DrawDottedLine(new Vector3(rect.xMin, rect.yMin), new Vector3(rect.xMax, rect.yMin), space * 4); // top
            Handles.DrawDottedLine(new Vector3(rect.xMin, rect.yMin), new Vector3(rect.xMin, rect.yMax), space); // left
            Handles.DrawDottedLine(new Vector3(rect.xMin, rect.yMax), new Vector3(rect.xMax, rect.yMax), space * 4); // bottom
            Handles.DrawDottedLine(new Vector3(rect.xMax, rect.yMin), new Vector3(rect.xMax, rect.yMax), space); // right
            Handles.color = Color.white;
        }

        public static void FolderField(SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUILayout.HelpBox("FolderField only works on string field.", MessageType.Error);
                return;
            }
            bool isEmpty = string.IsNullOrEmpty(property.stringValue);
            using (var scope = new EditorGUILayout.HorizontalScope())
            {
                //if (isEmpty)
                //{
                //    EditorGUILayout.LabelField(EditorGUIUtility.TrTextContentWithIcon(label.text, MessageType.Error), new GUIContent("Folder not selected"), GUILayout.Height(18));
                //}
                //else
                //{
                //    EditorGUILayout.LabelField(label, new GUIContent(property.stringValue), GUILayout.Height(18));
                //}
                LabelField(label, new GUIContent(property.stringValue), isEmpty);
                //EditorGUILayout.LabelField(, GUILayout.Height(18));
                if (GUILayout.Button(EditorGUIUtility.TrIconContent("FolderOpened Icon", $"Choose output directory"), EditorStyles.label, GUILayout.Width(18), GUILayout.Height(18)))
                {
                    string absolutePath = EditorUtility.OpenFolderPanel("Choose output directory", DataDirectoryInfo.Parent.FullName, "");
                    if (!string.IsNullOrEmpty(absolutePath))
                    {
                        if (!absolutePath.StartsWith(DataDirectoryInfo.Parent.FullName.Replace('\\', '/')))
                        {
                            Debug.LogError($"The folder should be under the Project folder and outside the Assets folder.");
                        }
                        else if (absolutePath.StartsWith(Application.dataPath))
                        {
                            Debug.LogError($"The folder should NOT be under the Assets folder to avoid Unity generating the .meta files.");
                        }
                        else
                        {
                            property.stringValue = absolutePath;
                            GUI.FocusControl(null); // clear focus on textfield so the ui gets updated
                        }
                    }
                }
                //if (GUILayout.Button(EditorGUIUtility.TrIconContent("winbtn_win_close", "Reset"), EditorStyles.label, GUILayout.Width(18), GUILayout.Height(18)))
                //{
                //    property.stringValue = string.Empty;
                //}
                if (GUILayout.Button(EditorGUIUtility.TrIconContent("Folder Icon", $"Show in Explorer"), EditorStyles.label, GUILayout.Width(18), GUILayout.Height(18)))
                {
                    if (Directory.Exists(property.stringValue))
                    {
                        Application.OpenURL(property.stringValue);
                    }
                }
            }
        }

        public static void LabelField(GUIContent label, GUIContent label2, bool isRequired = false)
        {
            if (isRequired)
            {
                EditorGUILayout.LabelField(EditorGUIUtility.TrTextContentWithIcon(label.text, label.tooltip, MessageType.Error), label2);
            }
            else
            {
                EditorGUILayout.LabelField(label, label2);
            }
        }

        public static void PropertyField(SerializedProperty property, bool isRequired = false)
        {
            if (isRequired)
            {
                EditorGUILayout.PropertyField(property, EditorGUIUtility.TrTextContentWithIcon(property.displayName, property.tooltip, MessageType.Error));
            }
            else
            {
                EditorGUILayout.PropertyField(property);
            }
        }
    }
}
