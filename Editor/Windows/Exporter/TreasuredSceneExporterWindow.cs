using System;
using System.Collections.Generic;
using System.Linq;
using Treasured.UnitySdk.Validation;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using System.IO;

namespace Treasured.UnitySdk
{
    internal sealed class TreasuredSceneExporterWindow : UnityEditor.EditorWindow
    {
        class Styles
        {
            public static GUIContent[] tabs = new GUIContent[] { EditorGUIUtility.TrIconContent("console.infoicon"), EditorGUIUtility.TrIconContent("warning"), EditorGUIUtility.TrIconContent("error") };

            public static readonly GUIStyle exportButton = new GUIStyle("button")
            {
                padding = new RectOffset(8, 8, 8, 8),
                fontStyle = FontStyle.Bold,
            };
        }

        public class ListItem
        {
            public bool expanded = true;
            public ValidationResult validationResult;
        }

        public class MenuItem
        {
            public object target;
            public string path;
            public System.Action onGUI;
        }

        public TreasuredScene scene;
        public List<ListItem> results = new List<ListItem>();

        private ValidationResult.ValidationResultType _type = ValidationResult.ValidationResultType.Warning | ValidationResult.ValidationResultType.Error | ValidationResult.ValidationResultType.Info;
        private Vector2 listScrollPosition;
        private Vector2 editorScrollPosition;
        private bool hasError = false;

        ReorderableList reorderableList;
        List<Editor> editors;
        bool enableToggle;

        public static void Show(TreasuredScene scene)
        {
            var window = EditorWindow.GetWindow<TreasuredSceneExporterWindow>(true, "Treasured Scene Exporter", true);
            window.scene = scene;
            window.Initialize();
            window.RunPreExportCheck();
            window.Show();
        }

        private void OnEnable()
        {
            scene ??= Selection.activeGameObject.GetComponent<TreasuredScene>();
            if (scene)
            {
                Initialize();
                RunPreExportCheck();
            }
        }

        private void Initialize()
        {
            editors = new List<UnityEditor.Editor>();
            var types = TypeCache.GetTypesDerivedFrom<ExporterWindowMenuItem>();
            foreach (var type in types)
            {
                var menuItem = (ExporterWindowMenuItem)ScriptableObject.CreateInstance(type);
                var e = Editor.CreateEditor(menuItem);
                (e as ExporterWindowMenuItem).SetScene(scene);
                editors.Add(e);
            }
            foreach (var exporter in ReflectionUtilities.GetSerializableFieldValuesOfType<Exporter>(scene))
            {
                MenuItem menuItem = new MenuItem()
                {
                    path = ObjectNames.NicifyVariableName(exporter.Value.GetType().Name),
                    target = Editor.CreateEditor(exporter.Value)
                };
                menuItem.onGUI = () =>
                {
                    (menuItem.target as Editor).OnInspectorGUI();
                };
                editors.Add(Editor.CreateEditor(exporter.Value));
            }
            editors.Add(Editor.CreateEditor(scene.exportSettings));
            //MenuItem objects = new MenuItem();
            //objects.path = "Objects";
            //objects.target = scene;
            //objects.onGUI = () =>
            //{
            //    TreasuredScene scene = objects.target as TreasuredScene;
            //};
            //menuItems.Add(objects);

            reorderableList = new ReorderableList(editors, typeof(Editor));
            reorderableList.index = editors.FindIndex(x => x.target == scene.exportSettings);
            reorderableList.draggable = false;
            reorderableList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.BeginChangeCheck();
                enableToggle = GUI.Toggle(new Rect(rect.x, rect.y, 16, rect.height), enableToggle, GUIContent.none);
                if (EditorGUI.EndChangeCheck())
                {
                    foreach (var editor in editors)
                    {
                        if(editor.target is Exporter exporter)
                        {
                            exporter.enabled = enableToggle;
                        }
                    }
                }
                //if (GUI.Button(new Rect(rect.xMax - 64, rect.y, 64, rect.height), new GUIContent("Reset",
                //"Reset all exporter settings to default in Editor Preferences")))
                //{
                //    try
                //    {
                //        foreach (var editor in editors)
                //        {
                //            if (!(editor.target is Exporter exporter)) continue;
                //            var source = TreasuredSDKPreferences.Instance.exporters.FirstOrDefault(e =>
                //                e.GetType() == exporter.GetType());
                //            if (source == null)
                //            {
                //                continue;
                //            }
                //            SerializedObject serializedObject = new SerializedObject(source);
                //            SerializedProperty current = serializedObject.GetIterator();
                //            while (current.Next(true))
                //            {
                //                editor.serializedObject.CopyFromSerializedProperty(current);
                //            }
                //            editor.serializedObject.ApplyModifiedProperties();
                //        }
                //    }
                //    catch (Exception e)
                //    {
                //        throw e;
                //    }
                //}
            };
            reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                if ((editors[index].target is Exporter exporter))
                {
                    exporter.enabled = EditorGUI.Toggle(new Rect(rect.x, rect.y, 16, rect.height), exporter.enabled);
                    EditorGUI.LabelField(new Rect(rect.x + 16, rect.y, rect.width - 16, rect.height), ObjectNames.NicifyVariableName(exporter.GetType().Name));
                }
                else
                {
                    EditorGUI.LabelField(rect, ObjectNames.NicifyVariableName(editors[index].target.GetType().Name));
                }
            };
            reorderableList.displayAdd = false;
            reorderableList.displayRemove = false;
        }

        void RunPreExportCheck()
        {
            var fieldValues = ReflectionUtilities.GetSerializableFieldValuesOfType<Exporter>(scene);
            List<ValidationResult> validationResults = new List<ValidationResult>();
            foreach (var field in fieldValues)
            {
                var exporter = field.Value;
                var results = exporter.CanExport();
                if (results != null)
                    validationResults.AddRange(results);
            }
            if ((!TreasuredSDKPreferences.Instance.ignoreWarnings && validationResults.Count > 0) || (TreasuredSDKPreferences.Instance.ignoreWarnings && validationResults.Any(result => result.type == ValidationResult.ValidationResultType.Error)))
            {
                var e = new ValidationException(validationResults);
                this.results = e.results.Select(result => new ListItem() { validationResult = result }).OrderBy(x => x.validationResult.priority).ToList();
                Styles.tabs[0].text = $"({e.infos.Count})";
                Styles.tabs[1].text = $"({e.warnings.Count})";
                Styles.tabs[2].text = $"({e.errors.Count})";
                this.hasError = e.infos.Count > 0;
            }
        }

        private void OnGUI()
        {
            DefaultStyles.Init();
            using (new EditorGUILayout.VerticalScope())
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUILayout.VerticalScope())
                    {
                        scene.exportSettings.ExportType = (ExportType)EditorGUILayout.EnumPopup(new GUIContent("Export Type"), scene.exportSettings.ExportType, GUILayout.Width(240));
                    }
                    if (GUILayout.Button("Export", GUILayout.Height(32), GUILayout.Width(100)))
                    {
                        Exporter.ForceExport(scene);
                    }
                }
                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUILayout.VerticalScope(GUILayout.ExpandHeight(true), GUILayout.Width(200)))
                    {
                        reorderableList.DoLayoutList();
                    }
                    using (var scope = new EditorGUILayout.ScrollViewScope(editorScrollPosition, EditorStyles.helpBox))
                    {
                        editorScrollPosition = scope.scrollPosition;
                        if (reorderableList.index >= 0 && reorderableList.index < reorderableList.list.Count)
                        {
                            var editor = editors[reorderableList.index];
                            EditorGUILayout.LabelField(ObjectNames.NicifyVariableName(editor.target.GetType().Name), EditorStyles.boldLabel);
                            editor.OnInspectorGUI();
                        }
                    }
                }
            }
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.FlexibleSpace();
                for (int i = 0; i < Styles.tabs.Length; i++)
                {
                    var selectedType = (ValidationResult.ValidationResultType)Enum.GetValues(typeof(ValidationResult.ValidationResultType)).GetValue(i);
                    bool isSelected = _type.HasFlag(selectedType);
                    EditorGUI.BeginChangeCheck();
                    GUILayout.Toggle(isSelected, Styles.tabs[i], EditorStyles.toolbarButton);
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (isSelected)
                        {
                            _type &= ~selectedType;
                        }
                        else
                        {
                            _type |= selectedType;
                        }
                    }
                }
            }
            using (var scope = new EditorGUILayout.ScrollViewScope(listScrollPosition, GUILayout.MinHeight(120)))
            {
                listScrollPosition = scope.scrollPosition;
                ShowList(results);
            }
        }

        private void ShowList(List<ListItem> items)
        {
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (!_type.HasFlag(item.validationResult.type))
                {
                    continue;
                }
                item.expanded = EditorGUILayout.BeginFoldoutHeaderGroup(item.expanded, EditorGUIUtility.TrTextContentWithIcon(item.validationResult.name, GetMessageType(item.validationResult.type)));
                if (item.expanded)
                {
                    using (new EditorGUI.IndentLevelScope(2))
                    {
                        EditorGUILayout.LabelField(new GUIContent(item.validationResult.description), EditorStyles.wordWrappedLabel);
                        //EditorGUILayout.Space();
                        if (item.validationResult.resolvers != null)
                        {
                            using (new EditorGUILayout.VerticalScope())
                            {
                                EditorGUILayout.Space();
                                foreach (var resolver in item.validationResult.resolvers)
                                {
                                    using (new EditorGUILayout.HorizontalScope())
                                    {
                                        EditorGUILayout.Space();
                                        if (GUILayout.Button(resolver.text, DefaultStyles.Link))
                                        {
                                            resolver.onResolve?.Invoke();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
            }
        }
        
        private MessageType GetMessageType(ValidationResult.ValidationResultType validationResultType)
        {
            switch (validationResultType)
            {
                case ValidationResult.ValidationResultType.Info:
                    return MessageType.Info;
                case ValidationResult.ValidationResultType.Error:
                    return MessageType.Error;
                case ValidationResult.ValidationResultType.Warning:
                    return MessageType.Warning;
                default:
                    return MessageType.None;
            }
        }
    }
}
