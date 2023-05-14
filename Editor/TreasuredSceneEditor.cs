using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Treasured.UnitySdk.Validation;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [CustomEditor(typeof(TreasuredScene))]
    internal class TreasuredSceneEditor : UnityEditor.Editor
    {
        private const string SelectedTabIndexKey = "TreasuredSDK_Inspector_SelectedTabIndex";

        public static class Styles
        {
            public static readonly GUIContent alignView = EditorGUIUtility.TrTextContent("Align View");
            public static readonly GUIContent snapAllToGround = EditorGUIUtility.TrTextContent("Snap All on Ground");

            public static readonly GUIContent searchObjects =
                EditorGUIUtility.TrTextContent("Search", "Search objects by Id or name");

            public static readonly GUIContent folderOpened =
                EditorGUIUtility.TrIconContent("FolderOpened Icon", "Show in Explorer");

            public static readonly GUIContent search = EditorGUIUtility.TrIconContent("Search Icon");

            public static readonly GUIContent plus = EditorGUIUtility.TrIconContent("Toolbar Plus");
            public static readonly GUIContent minus = EditorGUIUtility.TrIconContent("Toolbar Minus", "Remove");

            public static readonly GUIStyle logoText = new GUIStyle(EditorStyles.boldLabel)
            {
                wordWrap = false,
                fontSize = 18,
                alignment = TextAnchor.UpperCenter,
                normal = { textColor = Color.white }
            };

            public static readonly GUIStyle objectLabel = new GUIStyle("label")
            {
                fontStyle = FontStyle.Bold
            };

            public static readonly GUIStyle button = new GUIStyle("label")
            {
                margin = new RectOffset(),
                padding = new RectOffset(),
            };

            public static readonly GUIStyle iconButton = new GUIStyle("button")
            {
                margin = new RectOffset(0, 0, 0, 8),
                fixedHeight = 24,
                fontStyle = FontStyle.Bold,
            };

            public static readonly GUIStyle dataDisplayBox = new GUIStyle("box")
            {
                margin = new RectOffset(0, 0, 0, 0),
                padding = new RectOffset(8, 8, 8, 8),
                alignment = TextAnchor.UpperLeft,
                fontSize = 10
            };

            public static readonly GUIStyle centeredLabel = new GUIStyle("label")
            {
                alignment = TextAnchor.UpperCenter,
                wordWrap = true,
            };

            public static readonly GUIStyle exportButton = new GUIStyle("button")
            {
                padding = new RectOffset(8, 8, 8, 8),
                fontStyle = FontStyle.Bold,
            };

            public static readonly GUIStyle noLabel = new GUIStyle("label")
            {
                fixedWidth = 1
            };

            public static readonly GUIContent ToolDescription = new GUIContent("Treasured is a tool to help you create and export your Unity scenes to the web. For more information, visit treasured.dev for more info");

            private static GUIStyle tabButton;

            public static GUIStyle TabButton
            {
                get
                {
                    if (tabButton == null)
                    {
                        tabButton = new GUIStyle(EditorStyles.toolbarButton)
                        {
                            fixedHeight = 32,
                            fontStyle = FontStyle.Bold,
                            padding = new RectOffset(8, 8, 0, 0),
                            normal =
                            {
                                background = Texture2D.blackTexture
                            },
                            onNormal =
                            {
                                background = CreateTexture2D(1, 1,
                                    new Color(Color.gray.r, Color.gray.g, Color.gray.b, 0.15f))
                            }
                        };
                    }

                    return tabButton;
                }
            }

            private static GUIStyle borderlessBoxOdd;

            /// <summary>
            /// Box without margin
            /// </summary>
            public static GUIStyle BorderlessBoxOdd
            {
                get
                {
                    if (borderlessBoxOdd == null)
                    {
                        borderlessBoxOdd = new GUIStyle("box")
                        {
                            margin = new RectOffset(0, 0, 2, 0),
                            padding = new RectOffset(0, 0, 6, 6),
                            normal = new GUIStyleState()
                            {
                                background = CreateTexture2D(1, 1, new Color(1, 1, 1, 0.05f))
                            }
                        };
                    }

                    return borderlessBoxOdd;
                }
            }

            private static GUIStyle borderlessBoxEven;

            /// <summary>
            /// Box without margin
            /// </summary>
            public static GUIStyle BorderlessBoxEven
            {
                get
                {
                    if (borderlessBoxEven == null)
                    {
                        borderlessBoxEven = new GUIStyle("box")
                        {
                            margin = new RectOffset(0, 0, 2, 0),
                            padding = new RectOffset(0, 0, 6, 6)
                        };
                    }

                    return borderlessBoxEven;
                }
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

        [AttributeUsage(AttributeTargets.Method)]
        public class TabGroupAttribute : Attribute
        {
            public string groupName;
            public int order;
        }

        public class TabGroupState
        {
            public TabGroupAttribute attribute;
            public MethodInfo gui;
            public int tabIndex;
        }

        public enum GroupToggleState
        {
            None,
            All,
            Mixed
        }

        public class FoldoutGroupState
        {
            public string groupName;
            public bool expanded;
            public Type type;
            public List<TreasuredObject> objects;
            public Vector2 scrollPosition;
            public GroupToggleState toggleState;
            public bool enableAll;
        }

        private TabGroupState[] _tabGroupStates;
        private int _selectedTabIndex = 1;
        private FoldoutGroupState[] _foldoutGroupStates;

        private TreasuredScene scene;
        private Exporter[] _exporters;

        private Process _npmProcess;

        private Dictionary<UnityEngine.Object, Editor> _cachedEditors = new Dictionary<UnityEngine.Object, Editor>();

        private bool _backgroudMusicExpanded = true;

        public void OnEnable()
        {
            _selectedTabIndex = SessionState.GetInt(SelectedTabIndexKey, _selectedTabIndex);
            scene = target as TreasuredScene;
            InitializeScriptableObjects();
            CreateCachedEditors();
            InitializeTabGroups();
            InitializeObjectList();
            ValidateSchema();
            try
            {
                var process = Process.GetProcessById(SessionState.GetInt(SessionKeys.CLIProcessId, -1));
                _npmProcess = process.HasExited ? null : process;
            }
            catch (Exception)
            {

            }
        }

        private void ValidateSchema()
        {
            int totalUpdated = 0;
            TreasuredObject[] objects = scene.GetComponentsInChildren<TreasuredObject>(true);
            foreach (var obj in objects)
            {
                if (!obj.actionGraph.TryGetActionGroup("onSelect", out var onSelect))
                {
                    onSelect = obj.actionGraph.AddActionGroup("onSelect");
                }
                //if (obj.OnClick.Count > 0)
                //{
                //    foreach (var actionGroup in obj.OnClick)
                //    {
                //        if (actionGroup == null || actionGroup.Actions == null) { continue; }
                //        if (actionGroup.Actions.Count > 1)
                //        {
                //            if (!onSelect.Contains(actionGroup.Id))
                //            {
                //                GroupAction group = new GroupAction();
                //                group.Id = actionGroup.Id;
                //                foreach (var action in actionGroup.Actions)
                //                {
                //                    ScriptableAction scriptableAction = action.ConvertToScriptableAction();
                //                    scriptableAction.Id = action.Id;
                //                    if (scriptableAction != null)
                //                    {
                //                        group.actions.Add(scriptableAction);
                //                    }
                //                }
                //                onSelect.Add(group);
                //                actionGroup.Actions.Clear();
                //                totalUpdated++;
                //            }
                //        }
                //        else if (actionGroup.Actions.Count == 1)
                //        {
                //            var firstAction = actionGroup.Actions[0];
                //            if (!onSelect.Contains(firstAction.Id))
                //            {
                //                ScriptableAction scriptableAction = firstAction.ConvertToScriptableAction();
                //                scriptableAction.Id = firstAction.Id;
                //                if (scriptableAction != null)
                //                {
                //                    onSelect.Add(scriptableAction);
                //                    totalUpdated++;
                //                    actionGroup.Actions.Clear();
                //                }
                //            }
                //        }
                //    }
                //}
                if (!obj.onClick.IsNullOrNone() && obj.onClick.Count > 0)
                {
                    List<int> indices = new List<int>();
                    for (int i = 0; i < obj.onClick.Count; i++)
                    {

                    }
                    foreach (var action in obj.onClick)
                    {
                        if (action != null && !onSelect.Contains(action.Id))
                        {
                            onSelect.Add(action);
                            totalUpdated++;
                        }
                    }
                    obj.onClick.Clear();
                }
            }
            foreach (var video in scene.Videos)
            {
                video.videoInfo = new VideoInfo()
                {
                    volume = video.volume,
                    autoplay = video.autoplay,
                    loop = video.loop,
                    Uri = video.src
                };
            }
            foreach (var sound in scene.Audios)
            {
                sound.audioInfo = new AudioInfo()
                {
                    volume = sound.volume,
                    autoplay = false,
                    loop = sound.loop,
                    Uri = sound.src
                };
            }
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
            if (totalUpdated > 0)
            {
                EditorUtility.DisplayDialog("Action Schema Update", $"Updated {totalUpdated} action(s) to Version 2.", "Ok");
            }
        }

        private void InitializeScriptableObjects()
        {
            var pairs = ReflectionUtilities.GetSerializableFieldValuesOfType<ScriptableObject>(scene);
            foreach (var pair in pairs)
            {
                if (pair.Value.IsNullOrNone())
                {
                    pair.Value = ScriptableObject.CreateInstance(pair.FieldInfo.FieldType);
                }
                if (pair.Value is Exporter exporter && exporter.Scene != scene)
                {
                    exporter.Scene = scene;
                }
            }
        }

        private void CreateCachedEditors()
        {
            // Create editors for export setting
            _cachedEditors[scene.exportSettings] = Editor.CreateEditor(scene.exportSettings);
            // Create editors for exporters
            var exporters = ReflectionUtilities.GetSerializableFieldValuesOfType<Exporter>(target, false);
            _exporters = exporters.Select(exporter => exporter.Value).ToArray();
            foreach (var exporter in _exporters)
            {
                _cachedEditors[exporter] = Editor.CreateEditor(exporter);
            }
            // Create editor for guided tour graph
            _cachedEditors[scene.graph] = Editor.CreateEditor(scene.graph);
            if (_cachedEditors[scene.graph] is GuidedTourGraphEditor editor)
            {
                editor.Scene = scene;
            }
        }

        private void InitializeTabGroups()
        {
            var guiMethods = GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(x => x.IsDefined(typeof(TabGroupAttribute))).ToArray();
            _tabGroupStates = new TabGroupState[guiMethods.Length];
            for (int i = 0; i < guiMethods.Length; i++)
            {
                _tabGroupStates[i] = new TabGroupState()
                {
                    tabIndex = i,
                    attribute = guiMethods[i].GetCustomAttribute<TabGroupAttribute>(),
                    gui = guiMethods[i]
                };
            }
        }

        private void InitializeObjectList()
        {
            var types = typeof(TreasuredObject).Assembly.GetTypes()
                .Where(x => !x.IsAbstract && typeof(TreasuredObject).IsAssignableFrom(x)).ToArray();
            _foldoutGroupStates = new FoldoutGroupState[types.Length];
            for (int i = 0; i < types.Length; i++)
            {
                _foldoutGroupStates[i] = new FoldoutGroupState()
                {
                    groupName = ObjectNames.NicifyVariableName(types[i].Name + "s"),
                    expanded = true,
                    type = types[i]
                };
                var objects = scene.GetComponentsInChildren(types[i], true);
                _foldoutGroupStates[i].objects = new List<TreasuredObject>();
                for (int x = 0; x < objects.Length; x++)
                {
                    _foldoutGroupStates[i].objects.Add((TreasuredObject)objects[x]);
                }
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            Texture2D TreasuredLogo = Resources.Load<Texture2D>("Treasured_Logo");
            EditorGUILayout.Space(10);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label(TreasuredLogo, GUILayout.Width(42f), GUILayout.Height(42f));
                GUILayout.Space(4);
                using (new EditorGUILayout.VerticalScope())
                {
                    GUILayout.Space(12);
                    GUILayout.Label("Treasured Unity SDK", Styles.logoText);
                    GUILayout.Space(12);
                }

                GUILayout.FlexibleSpace();
            }

            GUILayout.Space(10);
            GUILayout.Label(Styles.ToolDescription, Styles.centeredLabel);
            GUILayout.Space(10);

            // Draw Directory, Export and Play buttons
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                using (new EditorGUI.DisabledGroupScope(!Directory.Exists(scene.exportSettings.OutputDirectory)))
                {
                    if (GUILayout.Button(
                            EditorGUIUtility.TrTextContent("Directory ↗",
                                "Open the current output folder in the File Explorer. This function is enabled when the directory exist."),
                            Styles.exportButton, GUILayout.MaxWidth(150)))
                    {
                        EditorUtility.OpenWithDefaultApp(scene.exportSettings.OutputDirectory);
                    }
                }

                GUILayout.Space(10f);
                using (new EditorGUI.DisabledGroupScope(String.IsNullOrEmpty(scene.exportSettings.OutputDirectory) ||
                                                        !Regex.Match(scene.exportSettings.OutputDirectory,
                                                            @"[a-zA-Z0-9\-]").Success))
                {
                    if (GUILayout.Button(
                            EditorGUIUtility.TrTextContentWithIcon(scene.exportSettings.ExportType == ExportType.Export ? "Export" : "Production Export",
                                $"Export scene to {TreasuredSDKPreferences.Instance.customExportFolder}/{scene.exportSettings.folderName}",
                                "SceneLoadIn"), Styles.exportButton, scene.exportSettings.ExportType == ExportType.ProductionExport ? GUILayout.MaxWidth(200) : GUILayout.MaxWidth(150)))
                    {
                        try
                        {
                            Exporter.Export(scene);
                        }
                        catch (ValidationException e)
                        {
                            SceneExporterWindow.Show(scene, e);
                        }
                        catch (ContextException e)
                        {
                            EditorGUIUtility.PingObject(e.Context);
                            UnityEngine.Debug.LogException(e);
                        }
                        catch (TreasuredException e)
                        {
                            EditorGUIUtility.PingObject(scene);
                            UnityEngine.Debug.LogException(e);
                        }
                        catch (OperationCanceledException e)
                        {
                            EditorUtility.DisplayDialog("Canceled", e.Message, "OK");
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                        finally
                        {
                            EditorUtility.ClearProgressBar();
                        }
                    }
                    
                    if (EditorGUILayout.DropdownButton(EditorGUIUtility.TrTextContentWithIcon("", "Select Export type", "d_icon dropdown"), FocusType.Passive, Styles.exportButton, GUILayout.Height(30)))
                    {
                        GenericMenu menu = new GenericMenu();
                        menu.AddItem(new GUIContent("Export"), false, HandleItemClicked, ExportType.Export);
                        menu.AddItem(new GUIContent("Production Export"), false, HandleItemClicked, ExportType.ProductionExport);
                        menu.ShowAsContext();
                        
                        void HandleItemClicked(object parameter)
                        {
                            scene.exportSettings.ExportType = (ExportType)parameter;
                        }
                    }
                }

                GUILayout.Space(10f);
                using (new EditorGUI.DisabledGroupScope(!Directory.Exists(scene.exportSettings.OutputDirectory)))
                {
                    if (GUILayout.Button(
                        EditorGUIUtility.TrTextContent("Build",
                            "Build the export and host it on the server. This function is enabled when the directory exist.", "d_BuildSettings.Web.Small"),
                        Styles.exportButton, GUILayout.MaxWidth(150)))
                    {
                        var buildProcess =
                            ProcessUtilities.CreateProcess(
                                $"treasured build \"{scene.exportSettings.OutputDirectory}\"");
                        
                        string stdOutput = "";
                        string stdError = "";
                        
                        try
                        {
                            buildProcess.Start();
                            while (!buildProcess.HasExited)
                            {
                                if (EditorUtility.DisplayCancelableProgressBar("Building",
                                    "Please wait. Getting build ready.", 50 / 100f))
                                {
                                    ProcessUtilities.KillProcess(buildProcess);
                                    throw new OperationCanceledException();
                                }
                            }
                            stdOutput = buildProcess.StandardOutput.ReadToEnd();
                            stdError = buildProcess.StandardError.ReadToEnd();

                            EditorUtility.DisplayDialog("Build Finished", $"Build generated successfully.", "OK");
                            
                            EditorUtility.OpenWithDefaultApp(TreasuredSDKPreferences.Instance.customExportFolder);
                        }
                        catch (OperationCanceledException e)
                        {
                            EditorUtility.DisplayDialog("Canceled", e.Message, "OK");
                        }
                        catch (Exception e)
                        {
                            throw new ApplicationException(e.Message);
                        }
                        finally
                        {
                            if (!string.IsNullOrEmpty(stdOutput))
                            {
                                UnityEngine.Debug.Log(stdOutput);
                            }

                            if (!string.IsNullOrEmpty(stdError))
                            {
                                UnityEngine.Debug.LogError(stdError);
                            }

                            buildProcess?.Dispose();
                            EditorUtility.ClearProgressBar();
                        }
                    }
                }
                
                GUILayout.Space(10f);
                using (var playButtonGroup = new EditorGUILayout.FadeGroupScope(Convert.ToSingle(_npmProcess == null)))
                {
                    if (playButtonGroup.visible)
                    {
                        Color oldColor = GUI.backgroundColor;
                        GUI.backgroundColor = new Color(0.3f, 1.0f, 0.6f);
                        using (new EditorGUI.DisabledGroupScope(!Directory.Exists(scene.exportSettings.OutputDirectory)))
                        {
                            if (GUILayout.Button(
                                    EditorGUIUtility.TrTextContentWithIcon("Play", "Run in browser", "d_PlayButton On"),
                                    Styles.exportButton, GUILayout.MaxWidth(150)))
                            {
                                // Run `treasured dev` to start dev server
                                try
                                {
                                    _npmProcess =
                                        ProcessUtilities.CreateProcess(
                                            $"treasured dev {scene.exportSettings.folderName}");
                                    _npmProcess.Start();

                                    SessionState.SetInt(SessionKeys.CLIProcessId, _npmProcess.Id);

                                    UnityEditor.EditorApplication.quitting -= () => ProcessUtilities.KillProcess(_npmProcess);
                                    UnityEditor.EditorApplication.quitting += () => ProcessUtilities.KillProcess(_npmProcess);
                                }
                                catch (Exception e)
                                {
                                    UnityEngine.Debug.LogError(e);
                                    UnityEngine.Debug.LogError(e.Message);
                                }
                            }
                        }

                        GUI.backgroundColor = oldColor;
                    }
                }

                using (var playButtonGroup = new EditorGUILayout.FadeGroupScope(Convert.ToSingle(_npmProcess != null)))
                {
                    if (playButtonGroup.visible)
                    {
                        Color oldColor = GUI.backgroundColor;
                        GUI.backgroundColor = new Color(1.0f, 0.1f, 0.2f);
                        if (GUILayout.Button(
                                EditorGUIUtility.TrTextContentWithIcon("Stop", "Stop running server", "d_PreMatQuad"),
                                Styles.exportButton, GUILayout.MaxWidth(150)))
                        {
                            try
                            {
                                ProcessUtilities.KillProcess(_npmProcess);
                                _npmProcess = null;
                            }
                            catch (Exception e)
                            {
                                UnityEngine.Debug.LogError(e.Message);
                            }
                        }

                        GUI.backgroundColor = oldColor;
                    }
                }


                GUILayout.FlexibleSpace();
            }

            using (var scope = new EditorGUI.ChangeCheckScope())
            {
                _selectedTabIndex = GUILayout.SelectionGrid(_selectedTabIndex,
                    _tabGroupStates.Select(x => x.attribute.groupName).ToArray(), 3,
                    Styles.TabButton);
                if (scope.changed)
                {
                    SessionState.SetInt(SelectedTabIndexKey, _selectedTabIndex);
                }
            }

            for (int i = 0; i < _tabGroupStates.Length; i++)
            {
                var state = _tabGroupStates[i];
                if (state.tabIndex != _selectedTabIndex)
                {
                    continue;
                }

                state.gui.Invoke(this, null);
            }

            serializedObject.ApplyModifiedProperties();
        }

        [TabGroup(groupName = "Scene Info")]
        private void OnPageSettingsGUI()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("creator"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("title"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("description"));
            EditorGUILayoutUtils.PropertyFieldWithHeader(serializedObject.FindProperty("sceneInfo"));
            EditorGUILayoutUtils.PropertyFieldWithHeader(serializedObject.FindProperty("themeInfo"));
            EditorGUILayoutUtils.PropertyFieldWithHeader(serializedObject.FindProperty("uiSettings"));
            EditorGUILayoutUtils.PropertyFieldWithHeader(serializedObject.FindProperty("features"));
        }

        [TabGroup(groupName = "Scene Objects")]
        private void OnSceneManagementGUI()
        {
            if (GUILayout.Button(new GUIContent("Open Scene Editor"), GUILayout.Height(32)))
            {
                TreasuredSceneEditorWindow.ShowWindow(scene);
            }
        }

        [TabGroup(groupName = "Guided Tour")]
        private void OnGuidedTourGUI()
        {
            if (!scene.features.guidedTour)
            {
                using(new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.HelpBox(
                    "Guided Tour is currently disabled.",
                    MessageType.Warning);
                    if (GUILayout.Button("Enable", GUILayout.ExpandHeight(true)))
                    {
                        scene.features.guidedTour = true;
                        serializedObject.ApplyModifiedProperties();
                    }
                }
            }

            _cachedEditors[scene.graph].OnInspectorGUI();
        }

        [TabGroup(groupName = "Export Settings")]
        private void OnExportGUI()
        {
            EditorGUI.BeginChangeCheck();
            _cachedEditors[scene.exportSettings].OnInspectorGUI();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Exporter Settings", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(new GUIContent("Reset to Default",
                    "Reset all exporter settings to Default Preferences")))
            {
                try
                {
                    foreach (var exporter in _exporters)
                    {
                        var source = TreasuredSDKPreferences.Instance.exporters.FirstOrDefault(e =>
                            e.GetType() == exporter.GetType());
                        if (source == null)
                        {
                            continue;
                        }

                        SerializedObject serializedObject = new SerializedObject(source);
                        SerializedProperty current = serializedObject.GetIterator();
                        while (current.Next(true))
                        {
                            _cachedEditors[exporter].serializedObject.CopyFromSerializedProperty(current);
                        }
                        _cachedEditors[exporter].serializedObject.ApplyModifiedProperties();
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            EditorGUILayout.EndHorizontal();
            using (new EditorGUI.IndentLevelScope(1))
            {
                foreach (var exporter in _exporters)
                {
                    using (var scope = new ExporterEditor.ExporterScope(_cachedEditors[exporter].serializedObject.FindProperty(nameof(Exporter.enabled))))
                    {
                        _cachedEditors[exporter].OnInspectorGUI();
                    }
                }
            }
        }
    }
}