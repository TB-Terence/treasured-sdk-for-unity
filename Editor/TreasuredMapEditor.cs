﻿using System;
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
    [CustomEditor(typeof(TreasuredMap))]
    internal class TreasuredMapEditor : UnityEditor.Editor
    {
        private const string SelectedTabIndexKey = "TreasuredSDK_Inspector_SelectedTabIndex";

        [MenuItem("Tools/Treasured/Upgrade to Latest(Stable)", priority = 99)]
        static void UpgradeToStableVersion()
        {
            Client.Add("https://github.com/TB-Terence/treasured-sdk-for-unity.git#upm");
        }

        [MenuItem("Tools/Treasured/Upgrade to Latest(Experimental)", priority = 99)]
        static void UpgradeToExperimentalVersion()
        {
            Client.Add("https://github.com/TB-Terence/treasured-sdk-for-unity.git#exp");
        }

        [MenuItem("CONTEXT/TreasuredMap/Reset Migrate Info")]
        static void ResetMigrateInfo(MenuCommand command)
        {
            TreasuredMap map = (TreasuredMap)command.context;
            map.migrateInfo.shouldMigrate = true;
        }

        class TreasuredMapGizmosSettings
        {
            public bool enableCameraPreview = true;
        }

        public static class Styles
        {
            public static readonly GUIContent alignView = EditorGUIUtility.TrTextContent("Align View");
            public static readonly GUIContent snapAllToGround = EditorGUIUtility.TrTextContent("Snap All on Ground");
            public static readonly GUIContent selectAll = EditorGUIUtility.TrTextContent("Select All");

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

        private TreasuredMap _map;
        private Exporter[] _exporters;
        private List<Hotspot> _hotspots;
        private Editor editor1;

        private Process _npmProcess;

        private Dictionary<UnityEngine.Object, Editor> _cachedEditors = new Dictionary<UnityEngine.Object, Editor>();

        private bool _backgroudMusicExpanded = true;

        public void OnEnable()
        {
            _selectedTabIndex = SessionState.GetInt(SelectedTabIndexKey, _selectedTabIndex);
            _map = target as TreasuredMap;
            _hotspots = new List<Hotspot>(_map.Hotspots);
            InitializeScriptableObjects();
            CreateCachedEditors();
            InitializeTabGroups();
            InitializeObjectList();
            SceneView.duringSceneGui -= OnSceneViewGUI;
            SceneView.duringSceneGui += OnSceneViewGUI;
            Migrate();
            try
            {
                var process = Process.GetProcessById(SessionState.GetInt(SessionKeys.CLIProcessId, -1));
                _npmProcess = process.HasExited ? null : process;
            }
            catch (Exception)
            {

            }
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneViewGUI;
        }

        private void Migrate()
        {
            if (!_map.migrateInfo.shouldMigrate)
            {
                return;
            }

            TreasuredObject[] objects = _map.GetComponentsInChildren<TreasuredObject>(true);
            foreach (TreasuredObject obj in objects)
            {
                // initialize graph
                if (obj.actionGraph == null)
                {
                    obj.actionGraph = new Treasured.Actions.ActionGraph();
                }
                if (obj.actionGraph.TryGetActionGroup("onSelect", out var onSelect))
                {
                    onSelect.Clear();
                }
                else
                {
                    onSelect = obj.actionGraph.AddActionGroup("onSelect");
                }

                foreach (var actionGroup in obj.OnClick)
                {
                    if (actionGroup.Actions.Count > 1)
                    {
                        GroupAction group = new GroupAction();
                        foreach (var action in actionGroup.Actions)
                        {
                            ScriptableAction scriptableAction = action.ConvertToScriptableAction();
                            if (scriptableAction != null)
                            {
                                group.actions.Add(scriptableAction);
                            }
                        }
                        onSelect.Add(group);
                    }
                    else if (actionGroup.Actions.Count == 1)
                    {
                        ScriptableAction scriptableAction = actionGroup.Actions[0].ConvertToScriptableAction();
                        if (scriptableAction != null)
                        {
                            onSelect.Add(scriptableAction);
                        }
                    }
                }

                if (obj is Interactable interactable)
                {
                    if (obj.actionGraph.TryGetActionGroup("onHover", out var onHover))
                    {
                        onHover.Clear();
                    }
                    else
                    {
                        onHover = obj.actionGraph.AddActionGroup("onHover");
                    }
                    foreach (var actionGroup in obj.OnHover)
                    {
                        if (actionGroup.Actions.Count > 1)
                        {
                            GroupAction group = new GroupAction();
                            foreach (var action in actionGroup.Actions)
                            {
                                ScriptableAction scriptableAction = action.ConvertToScriptableAction();
                                if (scriptableAction != null)
                                {
                                    group.actions.Add(scriptableAction);
                                }
                            }
                            onHover.Add(group);
                        }
                        else if (actionGroup.Actions.Count == 1)
                        {
                            ScriptableAction scriptableAction = actionGroup.Actions[0].ConvertToScriptableAction();
                            if (scriptableAction != null)
                            {
                                onHover.Add(scriptableAction);
                            }
                        }
                    }
                }
            }

            _map.migrateInfo.shouldMigrate = false;
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }

        private void InitializeScriptableObjects()
        {
            var pairs = ReflectionUtilities.GetSerializableFieldValuesOfType<ScriptableObject>(_map);
            foreach (var pair in pairs)
            {
                if (pair.IsNull()
                {
                    pair.SetValue(ScriptableObject.CreateInstance(pair.FieldInfo.FieldType));
                }
                if (pair.GetValue() is Exporter exporter && exporter.Map != _map)
                {
                    exporter.Map = _map;
                }
            }
        }

        private void CreateCachedEditors()
        {
            // Create editors for export setting
            _cachedEditors[_map.exportSettings] = Editor.CreateEditor(_map.exportSettings);
            // Create editors for exporters
            var exporters = ReflectionUtilities.GetSerializableFieldValuesOfType<Exporter>(target, false);
            _exporters = exporters.Select(exporter => exporter.GetValueAs<Exporter>()).ToArray();
            foreach (var exporter in _exporters)
            {
                _cachedEditors[exporter] = Editor.CreateEditor(exporter);
            }
            // Create editor for guided tour graph
            _cachedEditors[_map.graph] = Editor.CreateEditor(_map.graph);
            if (_cachedEditors[_map.graph] is GuidedTourGraphEditor editor)
            {
                editor.Map = _map;
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
                var objects = _map.GetComponentsInChildren(types[i], true);
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
                using (new EditorGUI.DisabledGroupScope(!Directory.Exists(_map.exportSettings.OutputDirectory)))
                {
                    if (GUILayout.Button(
                            EditorGUIUtility.TrTextContent("Directory ↗",
                                "Open the current output folder in the File Explorer. This function is enabled when the directory exist."),
                            Styles.exportButton, GUILayout.MaxWidth(150)))
                    {
                        EditorUtility.OpenWithDefaultApp(_map.exportSettings.OutputDirectory);
                    }
                }

                GUILayout.Space(10f);
                using (new EditorGUI.DisabledGroupScope(String.IsNullOrEmpty(_map.exportSettings.OutputDirectory) ||
                                                        !Regex.Match(_map.exportSettings.OutputDirectory,
                                                            @"[a-zA-Z0-9\-]").Success))
                {
                    if (GUILayout.Button(
                            EditorGUIUtility.TrTextContentWithIcon(_map.exportSettings.ExportType == ExportType.Export ? "Export" : "Production Export",
                                $"Export scene to {TreasuredSDKPreferences.Instance.customExportFolder}/{_map.exportSettings.folderName}",
                                "SceneLoadIn"), Styles.exportButton, _map.exportSettings.ExportType == ExportType.ProductionExport ? GUILayout.MaxWidth(200) : GUILayout.MaxWidth(150)))
                    {
                        try
                        {
                            Exporter.Export(_map);
                        }
                        catch (ValidationException e)
                        {
                            SceneExporterWindow.Show(_map, e);
                        }
                        catch (ContextException e)
                        {
                            EditorGUIUtility.PingObject(e.Context);
                            UnityEngine.Debug.LogException(e);
                        }
                        catch (TreasuredException e)
                        {
                            EditorGUIUtility.PingObject(_map);
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
                            _map.exportSettings.ExportType = (ExportType)parameter;
                        }
                    }
                }

                GUILayout.Space(10f);
                using (new EditorGUI.DisabledGroupScope(!Directory.Exists(_map.exportSettings.OutputDirectory)))
                {
                    if (GUILayout.Button(
                        EditorGUIUtility.TrTextContent("Build",
                            "Build the export and host it on the server. This function is enabled when the directory exist.", "d_BuildSettings.Web.Small"),
                        Styles.exportButton, GUILayout.MaxWidth(150)))
                    {
                        var buildProcess =
                            ProcessUtilities.CreateProcess(
                                $"treasured build \"{_map.exportSettings.OutputDirectory}\"");
                        
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
                        using (new EditorGUI.DisabledGroupScope(!Directory.Exists(_map.exportSettings.OutputDirectory)))
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
                                            $"treasured dev {_map.exportSettings.folderName}");
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
                    _tabGroupStates.Select(x => x.attribute.groupName).ToArray(), _tabGroupStates.Length,
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

        private bool _backgroudMusicExpanded = true;

        [TabGroup(groupName = "Page Info")]
        private void OnPageInfoGUI()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_author"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_title"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_description"));
            _backgroudMusicExpanded = EditorGUILayout.Foldout(_backgroudMusicExpanded, "Background Music", true);
            if (_backgroudMusicExpanded)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_audioUrl"), new GUIContent("Uri", "Url to the audio file"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_muteOnStart"), new GUIContent("Mute", "Mute the audio at the beginning"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(TreasuredMap.defaultBackgroundVolume)), new GUIContent("Volume"));
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_templateLoader"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("headHTML"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pageEmbeds"));
            SerializedProperty uiSettings = serializedObject.FindProperty("uiSettings");
            EditorGUILayout.PropertyField(uiSettings);
            SerializedProperty features = serializedObject.FindProperty("features");
            EditorGUILayout.PropertyField(features);
        }

        [TabGroup(groupName = "Objects")]
        private void OnObjectsGUI()
        {
            EditorGUI.indentLevel++;
            for (int i = 0; i < _foldoutGroupStates.Length; i++)
            {
                using (new GUILayout.VerticalScope(i % 2 == 0 ? Styles.BorderlessBoxEven : Styles.BorderlessBoxOdd))
                {
                    var state = _foldoutGroupStates[i];
                    using (new GUILayout.HorizontalScope())
                    {
                        state.expanded = EditorGUILayout.Foldout(state.expanded, new GUIContent(state.groupName), true);
                    }

                    if (state.expanded)
                    {
                        using (new EditorGUILayout.VerticalScope())
                        {
                            if (state.objects.Count > 0)
                            {
                                using (new EditorGUILayout.HorizontalScope())
                                {
                                    EditorGUILayout.LabelField(
                                        new GUIContent(state.type == typeof(Hotspot) ? "Order" : string.Empty,
                                            state.type == typeof(Hotspot)
                                                ? "The order of the Hotspot for the Guide Tour."
                                                : string.Empty), GUILayout.Width(58));
                                    EditorGUILayout.LabelField(new GUIContent("Name"), GUILayout.Width(64));
                                    GUILayout.FlexibleSpace();
                                    if (GUILayout.Button(GUIIcons.menu, EditorStyles.label, GUILayout.Width(18),
                                            GUILayout.Height(20)))
                                    {
                                        ShowObjectListMenu(state.objects, state.type);
                                    }

                                    ;
                                }

                                using (new EditorGUILayout.HorizontalScope())
                                {
                                    int activeCount = state.objects.Count(x => x.gameObject.activeSelf);
                                    if (activeCount == state.objects.Count)
                                    {
                                        state.toggleState = TreasuredMapEditor.GroupToggleState.All;
                                        state.enableAll = true;
                                    }
                                    else
                                    {
                                        state.toggleState = activeCount == 0
                                            ? GroupToggleState.None
                                            : GroupToggleState.Mixed;
                                        state.enableAll = false;
                                    }

                                    EditorGUI.showMixedValue = state.toggleState == GroupToggleState.Mixed;
                                    GUILayout.Space(3);
                                    EditorGUI.BeginChangeCheck();
                                    state.enableAll = EditorGUILayout.ToggleLeft(GUIContent.none, state.enableAll);
                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        foreach (var obj in state.objects)
                                        {
                                            obj.gameObject.SetActive(state.enableAll);
                                        }
                                    }

                                    EditorGUI.showMixedValue = false;
                                }
                            }

                            if (state.objects.Count == 0)
                            {
                                EditorGUILayout.LabelField(
                                    $"No {ObjectNames.NicifyVariableName(state.type.Name)} Found",
                                    EditorStyles.centeredGreyMiniLabel);
                            }
                            else
                            {
                                using (var scope = new EditorGUILayout.ScrollViewScope(state.scrollPosition,
                                           GUILayout.Height(state.objects.Count == 0
                                               ? 20
                                               : Mathf.Clamp(state.objects.Count * 20, state.objects.Count * 20, 200))))
                                {
                                    state.scrollPosition = scope.scrollPosition;
                                    for (int index = 0; index < state.objects.Count; index++)
                                    {
                                        using (new EditorGUILayout.HorizontalScope())
                                        {
                                            TreasuredObject current = state.objects[index];
                                            // TODO: width 40 only show up to 10000
                                            EditorGUI.BeginChangeCheck();
                                            bool active = EditorGUILayout.Toggle(GUIContent.none,
                                                current.gameObject.activeSelf, GUILayout.Width(20));
                                            if (EditorGUI.EndChangeCheck())
                                            {
                                                current.gameObject.SetActive(active);
                                            }

                                            EditorGUILayout.LabelField($"{index + 1}", GUILayout.Width(32));
                                            using (var hs = new EditorGUILayout.HorizontalScope())
                                            {
                                                using (new EditorGUI.DisabledGroupScope(!current.gameObject.activeSelf))
                                                {
                                                    EditorGUILayout.LabelField(
                                                        new GUIContent(current.gameObject.name, current.Id),
                                                        style: Styles.objectLabel);
                                                }
                                            }

                                            switch (EditorGUILayoutUtils.CreateClickZone(Event.current,
                                                        GUILayoutUtility.GetLastRect(), MouseCursor.Link))
                                            {
                                                case 0:
                                                    if (current is Hotspot hotspot)
                                                    {
                                                        SceneView.lastActiveSceneView.LookAt(hotspot.Camera.transform.position, hotspot.Camera.transform.rotation, 0.01f);
                                                    }
                                                    else
                                                    {
                                                        // Always oppsite to the transform.forward
                                                        if (current.Hitbox != null)
                                                        {
                                                            Vector3 targetPosition = current.Hitbox.transform.position;
                                                            Vector3 cameraPosition = current.Hitbox.transform.position +
                                                                current.Hitbox.transform.forward * 1;
                                                            SceneView.lastActiveSceneView.LookAt(cameraPosition,
                                                                Quaternion.LookRotation(targetPosition -
                                                                    cameraPosition), 1);
                                                        }
                                                    }

                                                    EditorGUIUtility.PingObject(current);
                                                    break;
                                                case 1:
                                                    GenericMenu menu = new GenericMenu();
#if UNITY_2020_3_OR_NEWER
                                                    menu.AddItem(new GUIContent("Rename"), false,
                                                        () => { GameObjectUtils.RenameGO(current.gameObject); });
                                                    menu.AddSeparator("");
#endif
                                                    menu.AddItem(new GUIContent("Remove"), false, (obj) =>
                                                    {
                                                        var go = obj as TreasuredObject;
                                                        state.objects.Remove(go);
                                                        GameObject.DestroyImmediate(go.gameObject);
                                                    }, current);
                                                    menu.ShowAsContext();
                                                    break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            EditorGUI.indentLevel--;
        }

        [TabGroup(groupName = "Actions")]
        private void OnActionsGUI()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(TreasuredMap.onSceneLoad)));
        }

        [TabGroup(groupName = "Guided Tour")]
        private void OnGuidedTourGUI()
        {
            if (!_map.features.guidedTour)
            {
                EditorGUILayout.HelpBox(
                    "Guided Tour is currently disabled. You can go to Page Info > Features to turn it on.",
                    MessageType.Warning);
                return;
            }

            _cachedEditors[_map.graph].OnInspectorGUI();
        }

        [TabGroup(groupName = "Export Settings")]
        private void OnExportGUI()
        {
            EditorGUI.BeginChangeCheck();
            _cachedEditors[_map.exportSettings].OnInspectorGUI();
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

        void ShowObjectListMenu(IList<TreasuredObject> objects, Type type)
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(Styles.selectAll, false,
                () => { Selection.objects = _map.GetComponentsInChildren(type).Select(x => x.gameObject).ToArray(); });
            menu.ShowAsContext();
        }

        private void OnSceneViewGUI(SceneView view)
        {
            return;
            if (SceneView.lastActiveSceneView.size == 0.01f) // this happens when TreasuredObject is selected
            {
                return;
            }

            for (int i = 0; i < _map.Hotspots.Length; i++)
            {
                Hotspot current = _map.Hotspots[i];
                if (!current.gameObject.activeSelf)
                {
                    continue;
                }

                Hotspot next = GetNextActiveHotspot(i, _map.Hotspots);

                Transform hitboxTransform = current.Hitbox.transform;
                Transform cameraTransform = current.Camera.transform;

                if (Selection.activeGameObject != current.gameObject)
                {
                    Handles.color = Color.white;
                    Handles.DrawDottedLine(hitboxTransform.position, cameraTransform.position, 5);
                }

                if (!_map.Loop && i == _map.Hotspots.Length - 1)
                {
                    continue;
                }

                if (!next)
                {
                    continue;
                }

                Handles.color = Color.white;
                Handles.DrawLine(hitboxTransform.position, next.Hitbox.transform.position);
                Vector3 direction = next.Hitbox.transform.position - hitboxTransform.position;
                if (direction != Vector3.zero)
                {
                    Handles.color = Color.green;
                    Handles.ArrowHandleCap(0, hitboxTransform.position, Quaternion.LookRotation(direction), 0.5f,
                        EventType.Repaint);
                }
            }
        }

        private Hotspot GetNextActiveHotspot(int currentIndex, IList<Hotspot> list)
        {
            int index = currentIndex;
            Hotspot current = list[index];
            Hotspot next = list[(index + 1) % list.Count];
            while (next != current)
            {
                if (index == list.Count - 1 && !_map.Loop)
                {
                    return null;
                }

                if (next.gameObject.activeSelf)
                {
                    return next;
                }

                next = list[++index % list.Count];
            }

            return null;
        }
    }
}