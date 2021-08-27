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
            using (new EditorGUILayout.VerticalScope())
            {
                using (var scope = new EditorGUILayout.HorizontalScope())
                {
                    Rect rect = GUILayoutUtility.GetRect(scope.rect.width, 18, EditorStyles.textField);
                    rect = EditorGUI.PrefixLabel(rect, label);
                    EditorGUI.SelectableLabel(new Rect(rect.x, rect.y, rect.width - 36, rect.height), isEmpty ? "Not selected" : property.stringValue);
                    EditorGUI.EndDisabledGroup();
                    if (GUI.Button(new Rect(rect.x + rect.width - 36, rect.y, 18, rect.height), EditorGUIUtility.TrIconContent("FolderOpened Icon", $"Select folder"), EditorStyles.label))
                    {
                        string absolutePath = EditorUtility.OpenFolderPanel("Choose folder", DataDirectoryInfo.Parent.FullName, "");
                        if (!string.IsNullOrEmpty(absolutePath))
                        {
                            if (!absolutePath.StartsWith(DataDirectoryInfo.Parent.FullName.Replace('\\', '/')))
                            {
                                Debug.LogError($"The folder should be under the Project folder and outside the Assets folder.");
                            }
                            else if(absolutePath.StartsWith(Application.dataPath))
                            {
                                Debug.LogError($"The folder should NOT be under the Assets folder to avoid Unity generating the .meta files.");
                            }
                            else
                            {
                                property.stringValue = absolutePath;
                                GUI.FocusControl(null); // clear focus on textfield so the ui gets updated
                                //property.stringValue = AssetDatabase.AssetPathToGUID(selectedPath.Substring(Application.dataPath.Length - 6));
                            }
                        }
                    }
                    if (GUI.Button(new Rect(rect.x + rect.width - 18, rect.y, 18, rect.height), EditorGUIUtility.TrIconContent("winbtn_win_close", "Reset"), EditorStyles.label))
                    {
                        property.stringValue = "";
                    }
                }
            }
        }
    }
}
