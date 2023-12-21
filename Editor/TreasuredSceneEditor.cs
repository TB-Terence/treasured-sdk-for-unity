using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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
            SetDefaultTourIfNone();
            if (!scene.exportSettings.IsNullOrNone() && string.IsNullOrEmpty(scene.exportSettings.folderName))
            {
                scene.exportSettings.folderName = ObjectNames.NicifyVariableName(scene.gameObject.scene.name).Replace(" ", "-");
            }
            try
            {
                var process = Process.GetProcessById(SessionState.GetInt(SessionKeys.CLIProcessId, -1));
                _npmProcess = process.HasExited ? null : process;
            }
            catch (Exception)
            {

            }
        }

        void ValidateSchema()
        {
            Dictionary<Type, int> count = new Dictionary<Type, int>();
            Action<Type> increamentCount = (type) =>
            {
                if (!count.ContainsKey(type))
                {
                    count.Add(type, 1);
                }
                else
                {
                    count[type]++;
                };
            };
            foreach (var to in scene.GetComponentsInChildren<TreasuredObject>())
            {
                if (to.actionGraph.TryGetActionGroup("onSelect", out ActionCollection group))
                {
                    UpdateCollection<AudioAction>(group, (action) =>
                    {
                        if (string.IsNullOrEmpty(action.audioInfo.Path) && !string.IsNullOrEmpty(action.src))
                        {
                            action.audioInfo.Path = action.src;
                            increamentCount.Invoke(action.GetType());
                        }
                    });
                    UpdateCollection<CustomEmbedCodeAction>(group, (action) =>
                    {
                        if (string.IsNullOrEmpty(action.html.bodyHTML) && !string.IsNullOrEmpty(action.html.headHTML))
                        {
                            action.html.bodyHTML = action.html.headHTML;
                            increamentCount.Invoke(action.GetType());
                        }
                    });
                }
            }
            if (count.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var type in count)
                {
                    sb.AppendLine($"{ObjectNames.NicifyVariableName(type.Key.Name)} - {type.Value}");
                }
                if(EditorUtility.DisplayDialog("Action Update", $"Updated Action(s) using new schema\n\n{sb.ToString()}\nDo you want to export the newest data file?", "Export JSON Only", "Skip"))
                {
                    Exporter.ForceExport(scene, typeof(JsonExporter));
                }
            }
        }

        void UpdateCollection<T>(ActionCollection actionCollection, Action<T> actionToPerform) where T : ScriptableAction
        {
            foreach (ScriptableAction sa in actionCollection)
            {
                if (sa is T action)
                {
                    actionToPerform.Invoke(action);
                }
                else if (sa is GroupAction groupAction)
                {
                    UpdateCollection<T>(groupAction.actions, actionToPerform);
                }
            }
        }

        private void SetDefaultTourIfNone()
        {
            if (scene.graph.tours.Count > 0 && scene.graph.tours.All(x => x.isDefault == false))
            {
                scene.graph.tours[0].isDefault = true;
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
            // Create editor for guided tour graph
            //_cachedEditors[scene.graph] = Editor.CreateEditor(scene.graph);
            //if (_cachedEditors[scene.graph] is GuidedTourGraphEditor editor)
            //{
            //    editor.Scene = scene;
            //}
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
                //using (new EditorGUI.DisabledGroupScope(String.IsNullOrEmpty(scene.exportSettings.OutputDirectory) ||
                //                                        !Regex.Match(scene.exportSettings.OutputDirectory,
                //                                            @"[a-zA-Z0-9\-]").Success))
                {
                    if (GUILayout.Button(
                            EditorGUIUtility.TrTextContentWithIcon("Exporter",
                                $"Export scene to {TreasuredSDKPreferences.Instance.customExportFolder}/{scene.exportSettings.folderName}",
                                "SceneLoadIn"), Styles.exportButton, scene.exportSettings.ExportType == ExportType.Production ? GUILayout.MaxWidth(200) : GUILayout.MaxWidth(150)))
                    {
                        try
                        {
                            TreasuredSceneExporterWindow.Show(scene);
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

            if (GUILayout.Button(new GUIContent("Open Scene Editor"), GUILayout.Height(32)))
            {
                TreasuredSceneEditorWindow.ShowWindow(scene);
            }

            if (GUILayout.Button(new GUIContent("Open Guided Tour Editor"), GUILayout.Height(32)))
            {
                GuidedTourEditorWindow.ShowWindow(scene);
            }

            OnPageSettingsGUI();

            //using (var scope = new EditorGUI.ChangeCheckScope())
            //{
            //    _selectedTabIndex = GUILayout.SelectionGrid(_selectedTabIndex,
            //        _tabGroupStates.Select(x => x.attribute.groupName).ToArray(), 3,
            //        Styles.TabButton);
            //    if (scope.changed)
            //    {
            //        SessionState.SetInt(SelectedTabIndexKey, _selectedTabIndex);
            //    }
            //}

            //for (int i = 0; i < _tabGroupStates.Length; i++)
            //{
            //    var state = _tabGroupStates[i];
            //    if (state.tabIndex != _selectedTabIndex)
            //    {
            //        continue;
            //    }

            //    state.gui.Invoke(this, null);
            //}

            serializedObject.ApplyModifiedProperties();
        }

        bool pageInfoFoldoutState = true;

        private void OnPageSettingsGUI()
        {
            serializedObject.Update();
            pageInfoFoldoutState = EditorGUILayout.BeginFoldoutHeaderGroup(pageInfoFoldoutState, "Page Info");
            if (pageInfoFoldoutState)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(TreasuredScene.creator)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(TreasuredScene.title)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(TreasuredScene.description)));
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayoutUtils.PropertyFieldFoldout(serializedObject.FindProperty("sceneInfo"));
            EditorGUILayoutUtils.PropertyFieldFoldout(serializedObject.FindProperty("themeInfo"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(TreasuredScene.pageEmbeds)));
            EditorGUILayoutUtils.PropertyFieldFoldout(serializedObject.FindProperty("uiSettings"));
            EditorGUILayoutUtils.PropertyFieldFoldout(serializedObject.FindProperty("features"));
            serializedObject.ApplyModifiedProperties();
        }
    }
}