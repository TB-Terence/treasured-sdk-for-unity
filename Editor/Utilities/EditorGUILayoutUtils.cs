using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    internal static class EditorGUILayoutUtils
    {
        public sealed class GroupScope : GUI.Scope
        {
            public GroupScope(string groupName, GUIStyle style)
            {
                EditorGUILayout.LabelField(groupName, style);
                EditorGUI.indentLevel++;
            }

            protected override void CloseScope()
            {
                EditorGUI.indentLevel--;
            }
        }

        static class Styles
        {
            public static GUIStyle Link = new GUIStyle() { stretchWidth = false, normal = { textColor = new Color(0f, 0.47f, 0.85f) } };
            public static readonly GUIContent requiredField = EditorGUIUtility.TrIconContent("Error", "Required field");
        }

        private static object transformRotationGUI;
        private static MethodInfo transformRotationOnEnableMethodInfo;
        private static MethodInfo transformRotationGUIMethodInfo;

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

        /// <summary>
        /// Creates a clickable zone inside the given Rect.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="rect"></param>
        /// <param name="mouseCursor"></param>
        /// <param name="button"></param>
        /// <returns>Return -1 if nothing is clicked, 0 for left mouse buton, 1 for right mouse button and 2 for middle mouse button.</returns>
        public static int CreateClickZone(Event e, Rect rect, MouseCursor mouseCursor)
        {
            EditorGUIUtility.AddCursorRect(rect, mouseCursor);
            bool clicked = rect.Contains(e.mousePosition) && (e.type == EventType.MouseDown || e.type == EventType.MouseUp);
            if (clicked)
            {
                e.Use();
            }
            return clicked ? e.button : -1;
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

        /// <summary>
        /// Show Error icon if the field is missing.
        /// </summary>
        /// <param name="property"></param>
        /// <returns>Return true if data is missing otherwise false.</returns>
        public static bool RequiredPropertyField(SerializedProperty property)
        {
            bool missingData = false;
            using (new EditorGUILayout.HorizontalScope())
            {
                // TODO: possible turn this into Func<bool> with custom condition check
                switch (property.propertyType)
                {
                    case SerializedPropertyType.String:
                        missingData = string.IsNullOrWhiteSpace(property.stringValue);
                        break;
                }
                // fixes TextArea indent
                if (missingData)
                {
                    EditorGUILayout.PropertyField(property, EditorGUIUtility.TrTextContent(property.displayName, property.tooltip, "Error"));
                }
                else
                {
                    EditorGUILayout.PropertyField(property);
                }
            }
            return missingData;
        }

        public static void TransformPropertyField(SerializedObject serializedTransform, string name, bool showPosition = true, bool showRotation = true, bool showScale = true)
        {
            SerializedProperty property = serializedTransform.GetIterator();
            if (serializedTransform != null && property != null)
            {
                property.isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(property.isExpanded, name);
                if (property.isExpanded)
                {
                    using (new EditorGUI.IndentLevelScope(1))
                    {
                        EditorGUI.BeginChangeCheck();
                        serializedTransform.Update();
                        SerializedProperty localPosition = serializedTransform.FindProperty("m_LocalPosition");
                        SerializedProperty localRotation = serializedTransform.FindProperty("m_LocalRotation");
                        SerializedProperty localScale = serializedTransform.FindProperty("m_LocalScale");
                        if (showPosition && localPosition != null)
                        {
                            EditorGUILayout.PropertyField(localPosition);
                        }
                        if (showRotation && localRotation != null)
                        {
                            if (transformRotationGUI == null)
                            {
                                var transformRotationGUIType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.TransformRotationGUI");
                                transformRotationGUI = Activator.CreateInstance(transformRotationGUIType);
                                transformRotationOnEnableMethodInfo = transformRotationGUIType.GetMethod("OnEnable");
                                transformRotationGUIMethodInfo = transformRotationGUIType.GetMethods().FirstOrDefault(x => x.Name == "RotationField" && x.GetParameters().Length == 0);
                            }
                            transformRotationOnEnableMethodInfo.Invoke(transformRotationGUI, new object[] { serializedTransform.FindProperty("m_LocalRotation"), new GUIContent("Local Rotation") });
                            transformRotationGUIMethodInfo.Invoke(transformRotationGUI, null);
                        }
                        if (showScale && localScale != null)
                        {
                            EditorGUILayout.PropertyField(serializedTransform.FindProperty("m_LocalScale"));
                        }
                        serializedTransform.ApplyModifiedProperties();
                    }
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
            }
        }

        /// <summary>
        /// Draws a text field with folder picker and reset button.
        /// </summary>
        /// <param name="serializedProperty">String based serialized field.</param>
        /// <param name="title">The title of the OpenFolderPanel window.</param>
        /// <param name="folderPath">The path of the folder when it first show up.</param>
        /// <param name="fallbackPath">The path to use if the folder does not exist.</param>
        /// <exception cref="ArgumentException"></exception>
        public static void FolderField(SerializedProperty serializedProperty, string title, string folderPath = "", string fallbackPath = "")
        {
            if (serializedProperty.propertyType != SerializedPropertyType.String)
            {
                throw new ArgumentException($"Type mismatch. {serializedProperty} is not type of string.");
            }
            EditorGUI.BeginChangeCheck();
            using (new EditorGUILayout.HorizontalScope())
            {
                string newPath = EditorGUILayout.TextField(serializedProperty.displayName, serializedProperty.stringValue);
                if (GUILayout.Button(EditorGUIUtility.TrIconContent("FolderOpened Icon", $"Select custom export folder."), EditorStyles.label, GUILayout.Width(20), GUILayout.Height(20)))
                {
                    newPath = EditorUtility.OpenFolderPanel(title, folderPath, "");
                    if (!string.IsNullOrEmpty(newPath))
                    {
                        serializedProperty.stringValue = newPath.Replace("\\", "/");
                    }
                }
                if (GUILayout.Button(EditorGUIUtility.TrIconContent("winbtn_win_close", $"Reset"), EditorStyles.label, GUILayout.Width(20), GUILayout.Height(20)))
                {
                    serializedProperty.stringValue = fallbackPath.Replace("\\", "/");
                }
                if (EditorGUI.EndChangeCheck())
                {
                    if (Directory.Exists(newPath))
                    {
                        EditorGUI.FocusTextInControl(null);
                        serializedProperty.serializedObject.ApplyModifiedProperties();
                    }
                }
            }
        }
    }
}
