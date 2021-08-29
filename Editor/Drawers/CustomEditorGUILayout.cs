using System;
using System.IO;
using Treasured.UnitySdk.Editor;
using UnityEditor;
using UnityEngine;

namespace Treasured.SDKEditor
{
    [InitializeOnLoad]
    internal static class CustomEditorGUILayout
    {
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

        public static void FolderField(SerializedProperty property, GUIContent label, string suffix = "")
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUILayout.HelpBox("FolderField only works on string field.", MessageType.Error);
                return;
            }
            bool isEmpty = string.IsNullOrEmpty(property.stringValue);
            using (var scope = new EditorGUILayout.HorizontalScope())
            {
                LabelField(label, new GUIContent($"{property.stringValue}{suffix}"), isEmpty);
                GUILayout.Label(EditorGUIUtility.TrIconContent("_Help", tooltip: $"{property.stringValue}{suffix}"), GUILayout.Width(20));
                if (GUILayout.Button(EditorGUIUtility.TrIconContent("FolderOpened Icon", $"Choose output directory"), EditorStyles.label, GUILayout.Width(18), GUILayout.Height(18)))
                {
                    string absolutePath = EditorUtility.OpenFolderPanel("Choose output directory", Utility.ProjectPath, "");
                    if (!string.IsNullOrEmpty(absolutePath))
                    {
                        string projectPath = Utility.ProjectPath.Replace('\\', '/');
                        if (!absolutePath.StartsWith(projectPath))
                        {
                            Debug.LogError($"The folder should be under the root folder of the Project.");
                        }
                        else if (absolutePath.StartsWith(Application.dataPath))
                        {
                            Debug.LogError($"The folder should NOT be under the Assets folder to avoid Unity generating the .meta files.");
                        }
                        else
                        {
                            string relativePath = absolutePath.Substring(Utility.ProjectPath.Length);
                            property.stringValue = $"Assets/..{(string.IsNullOrEmpty(relativePath) ? "/" : relativePath)}";
                            
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
                    string absolutePath = Path.Combine(Utility.ProjectPath, property.stringValue);
                    if (Directory.Exists(absolutePath))
                    {
                        Application.OpenURL(absolutePath);
                    }
                }
            }
        }

        public static void LabelField(GUIContent label, GUIContent label2, bool isRequired = false)
        {
            if (isRequired)
            {
                EditorGUILayout.LabelField(EditorGUIUtility.TrTextContentWithIcon(label.text, label.tooltip, MessageType.Warning), label2);
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
                EditorGUILayout.PropertyField(property, EditorGUIUtility.TrTextContentWithIcon(property.displayName, property.tooltip, MessageType.Warning));
            }
            else
            {
                EditorGUILayout.PropertyField(property);
            }
        }
    }
}
