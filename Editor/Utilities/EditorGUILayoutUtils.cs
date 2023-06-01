using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Treasured.UnitySdk.Utilities;
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

        public static class Styles
        {
            public static GUIStyle Link = new GUIStyle() { stretchWidth = false, normal = { textColor = new Color(0f, 0.47f, 0.85f) } };
            public static readonly GUIContent requiredField = EditorGUIUtility.TrIconContent("Error", "Required field");
            public static readonly GUIContent transformLabel = new GUIContent("Transform");

            public static readonly GUIStyle componentCardName = new GUIStyle(EditorStyles.boldLabel)
            {
                wordWrap = false,
                fontSize = 18,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = Color.white },
                fixedHeight = 42
            };
            public static readonly GUIStyle componentCardDescription = new GUIStyle("label")
            {
                alignment = TextAnchor.UpperLeft,
                wordWrap = true,
                fontStyle = FontStyle.Italic
            };
            public static readonly GUIStyle componentCardBox = new GUIStyle("helpBox")
            {
                padding = new RectOffset(10, 10, 10, 10)
            };
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

        public static void CreateComponentDropZone<T>(Rect rect, Action<IList<UnityEngine.Object>> onDrop)
        {
            CreateComponentDropZone(rect, typeof(T), onDrop);
        }

        public static void CreateComponentDropZone(Rect rect, Type type, Action<IList<UnityEngine.Object>> onDrop)
        {
            Event e = Event.current;
            if (rect.Contains(e.mousePosition) && (e.type == EventType.DragUpdated || e.type == EventType.DragPerform))
            {
                IList components = new ArrayList();
                foreach (var go in DragAndDrop.objectReferences.OfType<GameObject>())
                {
                    if (go.TryGetComponent(type, out Component component))
                    {
                        components.Add(component);
                    }
                }
                bool hasComponent = components.Count > 0;
                DragAndDrop.visualMode = hasComponent ? DragAndDropVisualMode.Link : DragAndDropVisualMode.Rejected;
                if (e.type == EventType.DragPerform && hasComponent)
                {
                    DragAndDrop.AcceptDrag();
                    onDrop.Invoke((IList<UnityEngine.Object>)components);
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

        public static void ComponentTransformPropertyField(SerializedProperty component, SerializedObject serializedTransform, string name, bool showPosition = true, bool showRotation = true, bool showScale = true)
        {
            if (serializedTransform == null)
            {
                return;
            }
            EditorGUILayout.ObjectField(component, new GUIContent(name));
            serializedTransform.Update();
            EditorGUILayout.LabelField(Styles.transformLabel);
            using (new EditorGUI.IndentLevelScope(1))
            {
                SerializedProperty localPosition = serializedTransform.FindProperty("m_LocalPosition");
                SerializedProperty localRotation = serializedTransform.FindProperty("m_LocalRotation");
                SerializedProperty localScale = serializedTransform.FindProperty("m_LocalScale");
                TransformPropertyFields(localPosition, localRotation, localScale, showPosition, showRotation, showScale);
            }
            serializedTransform.ApplyModifiedProperties();
        }

        public static void TransformPropertyField(SerializedProperty serializedProperty, string name, bool showPosition = true, bool showRotation = true, bool showScale = true)
        {
            if (serializedProperty == null)
            {
                return;
            }
            EditorGUILayout.ObjectField(serializedProperty, new GUIContent(serializedProperty.displayName));
            if (serializedProperty.objectReferenceValue.IsNullOrNone())
            {
                return;
            }
            SerializedObject serializedObject = new SerializedObject(serializedProperty.objectReferenceValue);
            SerializedProperty localPosition = serializedObject.FindProperty("m_LocalPosition");
            SerializedProperty localRotation = serializedObject.FindProperty("m_LocalRotation");
            SerializedProperty localScale = serializedObject.FindProperty("m_LocalScale");
            serializedObject.Update();
            TransformPropertyFields(localPosition, localRotation, localScale, showPosition, showRotation, showScale);
            serializedObject.ApplyModifiedProperties();
        }

        private static void TransformPropertyFields(SerializedProperty localPosition, SerializedProperty localRotation, SerializedProperty localScale, bool showPosition = true, bool showRotation = true, bool showScale = true)
        {
            using (new EditorGUI.IndentLevelScope())
            {
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
                    transformRotationOnEnableMethodInfo.Invoke(transformRotationGUI, new object[] { localRotation, new GUIContent("Local Rotation") });
                    transformRotationGUIMethodInfo.Invoke(transformRotationGUI, null);
                }
                if (showScale && localScale != null)
                {
                    EditorGUILayout.PropertyField(localScale);
                }
            }
        }

        public static void InteractbleButtonPropertyField(SerializedProperty property)
        {
            if (property != null)
            {
                property.serializedObject.Update();
                EditorGUI.BeginChangeCheck();
                var expanded = EditorGUILayout.BeginFoldoutHeaderGroup(SessionState.GetBool(SessionKeys.ShowInteractableButtonFoldout, true), new GUIContent(property.displayName));
                if (EditorGUI.EndChangeCheck())
                {
                    SessionState.SetBool(SessionKeys.ShowInteractableButtonFoldout, expanded);
                }
                if (expanded)
                {
                    SerializedProperty assetProperty = property.FindPropertyRelative("asset");
                    SerializedProperty transformProperty = property.FindPropertyRelative("transform");
                    SerializedProperty buttonPreview = property.FindPropertyRelative("preview");
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(assetProperty);
                    if (EditorGUI.EndChangeCheck() && !property.serializedObject.isEditingMultipleObjects)
                    {
                        var buttonTransform = CreateButtonTransform(property.serializedObject.targetObject as TreasuredObject);
                        transformProperty.objectReferenceValue = buttonTransform;
                    }
                    if (property.serializedObject.isEditingMultipleObjects)
                    {
                        EditorGUILayout.HelpBox("Multi-Editing for Transform and Preview is disabled.", MessageType.Info);
                    }
                    else
                    {
                        if (!assetProperty.objectReferenceValue.IsNullOrNone())
                        {
                            TransformPropertyField(transformProperty, "Transform", true, true, true);
                            EditorGUILayout.PropertyField(buttonPreview);
                        }
                    }
                }
                property.serializedObject.ApplyModifiedProperties();
            }
        }

        private static Transform CreateButtonTransform(TreasuredObject parent)
        {
            if (parent == null)
            {
                return null;
            }
            var transform = parent.transform.Find("Button");
            if (transform == null || parent.transform != transform.parent.transform)
            {
                GameObject go = new GameObject("Button");
                transform = go.transform;
                transform.SetParent(parent.transform);
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
                transform.localScale = Vector3.one;
            }
            return transform;
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

        public static void FolderField(ref string path, string label)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                var newPath = path;
                using (new EditorGUI.DisabledGroupScope(true))
                {
                    newPath = EditorGUILayout.TextField(label, path);
                }
                if (GUILayout.Button(EditorGUIUtility.TrIconContent("FolderOpened On Icon"), EditorStyles.label, GUILayout.Width(20), GUILayout.Height(18)))
                {
                    newPath = EditorUtility.OpenFolderPanel(label, path, "");
                    if (!string.IsNullOrEmpty(newPath))
                    {
                        path = newPath.Replace("\\", "/");
                    }
                }
            }
        }

        public static void ComponentCard(Texture2D icon, string title, string description, string helpUrl = "")
        {
            using (new EditorGUILayout.VerticalScope(Styles.componentCardBox))
            {
                //using (new EditorGUILayout.VerticalScope())
                //{
                //    using (new EditorGUILayout.HorizontalScope())
                //    {
                //        GUILayout.Space(20);
                //        GUILayout.Label(icon, GUILayout.Width(42f), GUILayout.Height(42f));
                //        using (new EditorGUILayout.VerticalScope())
                //        {
                //            GUILayout.Label(title, Styles.componentCardName);
                //        }
                //        GUILayout.FlexibleSpace();
                //    }
                //    GUILayout.Label(description, Styles.componentCardDescription);
                //}
                //using (new EditorGUILayout.VerticalScope())
                //{
                //    using (new EditorGUILayout.HorizontalScope())
                //    {
                //        GUILayout.Space(20);
                //        GUILayout.Label(icon, GUILayout.Width(42f), GUILayout.Height(42f));
                //        using (new EditorGUILayout.VerticalScope())
                //        {
                //            GUILayout.Label(title, Styles.componentCardName);
                //        }
                //        GUILayout.FlexibleSpace();
                //    }
                //    GUILayout.Label(description, Styles.componentCardDescription);
                //}
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label(icon, GUILayout.Width(42f), GUILayout.Height(42f));
                    using (new EditorGUILayout.VerticalScope())
                    {
                        using (new EditorGUILayout.VerticalScope())
                        {
                            GUILayout.Label(title, Styles.componentCardName);
                        }
                        GUILayout.Label(description, Styles.componentCardDescription);
                    }
                }
                if (!string.IsNullOrEmpty(helpUrl))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("More Info", EditorStyles.linkLabel))
                        {
                            EditorUtility.OpenWithDefaultApp(helpUrl);
                        }
                    }
                }
            }
        }

        public static void PropertyFieldWithHeader(SerializedProperty property)
        {
            if (property.propertyType == SerializedPropertyType.Generic)
            {
                EditorGUILayout.LabelField(property.displayName, EditorStyles.boldLabel);
                using(new EditorGUI.IndentLevelScope(1))
                {
                    EditorGUIUtils.DrawPropertyWithoutFoldout(property);
                }
            }
            else
            {
                EditorGUILayout.PropertyField(property);
            }
        }
    }
}
