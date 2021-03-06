using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

        private static readonly string[] selectableObjectListNames = new string[] { "Hotspots", "Interactables", "Videos", "Sounds" };

        class TreasuredMapGizmosSettings
        {
            public bool enableCameraPreview = true;
        }

        public static class Styles
        {
            public static readonly GUIContent alignView = EditorGUIUtility.TrTextContent("Align View");
            public static readonly GUIContent snapAllToGround = EditorGUIUtility.TrTextContent("Snap All on Ground");
            public static readonly GUIContent selectAll = EditorGUIUtility.TrTextContent("Select All");
            public static readonly GUIContent searchObjects = EditorGUIUtility.TrTextContent("Search", "Search objects by Id or name");

            public static readonly GUIContent folderOpened = EditorGUIUtility.TrIconContent("FolderOpened Icon", "Show in Explorer");

            public static readonly GUIContent search = EditorGUIUtility.TrIconContent("Search Icon");

            public static readonly GUIContent plus = EditorGUIUtility.TrIconContent("Toolbar Plus");
            public static readonly GUIContent minus = EditorGUIUtility.TrIconContent("Toolbar Minus", "Remove");

            public static readonly GUIStyle objectLabel = new GUIStyle("label")
            {
                fontStyle = FontStyle.Bold
            };

            public static readonly GUIStyle button = new GUIStyle("label")
            {
                margin = new RectOffset(),
                padding = new RectOffset(),
            };

            public static readonly GUIStyle exportButton = new GUIStyle("button")
            {
                margin = new RectOffset(0, 0, 0, 8),
                fixedHeight = 24,
                fontStyle = FontStyle.Bold
            };

            public static readonly GUIStyle noLabel = new GUIStyle("label")
            {
                fixedWidth = 1
            };

            private static GUIStyle tabButton;
            public static GUIStyle TabButton
            {
                get
                {
                    if (tabButton == null)
                    {
                        tabButton = new GUIStyle(EditorStyles.toolbarButton)
                        {
                            fixedHeight = 24,
                            fontStyle = FontStyle.Bold,
                            padding = new RectOffset(8, 8, 0, 0),
                            normal =
                            {
                                background = Texture2D.blackTexture
                            },
                            onNormal =
                            {
                                background = CreateTexture2D(1, 1, new Color(Color.gray.r, Color.gray.g, Color.gray.b, 0.15f))
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
        private Editor _exportSettingsEditor;
        private Editor[] _exporterEditors;
        private List<Hotspot> _hotspots;

        public void OnEnable()
        {
            _selectedTabIndex = SessionState.GetInt(SelectedTabIndexKey, _selectedTabIndex);
            _map = target as TreasuredMap;
            _hotspots = new List<Hotspot>(_map.Hotspots);
            InitializeSettings();
            InitializeExporters();
            InitializeTabGroups();
            InitializeObjectList();
            SceneView.duringSceneGui -= OnSceneViewGUI;
            SceneView.duringSceneGui += OnSceneViewGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneViewGUI;
        }

        private void InitializeSettings()
        {
            SerializedProperty exportSettings = serializedObject.FindProperty(nameof(TreasuredMap.exportSettings));
            if (exportSettings.objectReferenceValue == null)
            {
                exportSettings.objectReferenceValue = ScriptableObject.CreateInstance<ExportSettings>();
            }
            SerializedObject settings = new SerializedObject(exportSettings.objectReferenceValue);
            SerializedProperty previousFoldername = serializedObject.FindProperty("_outputFolderName");
            if (!string.IsNullOrEmpty(previousFoldername.stringValue))
            {
                settings.FindProperty("folderName").stringValue = previousFoldername.stringValue;
                previousFoldername.stringValue = "";
            }
            Editor.CreateCachedEditor(exportSettings.objectReferenceValue, null, ref _exportSettingsEditor);
            settings.ApplyModifiedProperties();
            serializedObject.ApplyModifiedProperties();
        }

        private void InitializeExporters()
        {
            // Find all serializable exporter
            FieldInfo[] exporterFields = typeof(TreasuredMap).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).
                Where(x =>
                typeof(Exporter).IsAssignableFrom(x.FieldType) &&
                (x.IsPublic || (x.IsPrivate && x.IsDefined(typeof(SerializeField))))).ToArray();
            _exporterEditors = new Editor[exporterFields.Length];
            for (int i = 0; i < exporterFields.Length; i++)
            {
                FieldInfo fi = exporterFields[i];
                SerializedProperty exporterProperty = serializedObject.FindProperty(fi.Name);
                if (exporterProperty.objectReferenceValue == null)
                {
                    exporterProperty.objectReferenceValue = ScriptableObject.CreateInstance(fi.FieldType);
                }
                SerializedObject exporterObject = new SerializedObject(exporterProperty.objectReferenceValue);
                SerializedProperty mapProperty = exporterObject.FindProperty("_map");
                if (mapProperty.objectReferenceValue == null)
                {
                    mapProperty.objectReferenceValue = target;
                }
                exporterObject.ApplyModifiedProperties();
                Editor.CreateCachedEditor(exporterProperty.objectReferenceValue, null, ref _exporterEditors[i]);
            }
            serializedObject.ApplyModifiedProperties();
        }

        public void InitializeTabGroups()
        {
            var guiMethods = GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(x => x.IsDefined(typeof(TabGroupAttribute))).ToArray();
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

        public void InitializeObjectList()
        {
            var types = typeof(TreasuredObject).Assembly.GetTypes().Where(x => !x.IsAbstract && typeof(TreasuredObject).IsAssignableFrom(x)).ToArray();
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
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(EditorGUIUtility.TrTextContentWithIcon("Export", $"Export scene to {TreasuredSDKPreferences.Instance.customExportFolder}/{_map.exportSettings.folderName}", "SceneLoadIn"), Styles.exportButton))
                {
                    try
                    {
                        Exporter.Export(_map);
                    }
                    catch (ValidationException e)
                    {
                        MapExporterWindow.Show(_map, e);
                    }
                }
                using(new EditorGUI.DisabledGroupScope(!Directory.Exists(_map.exportSettings.OutputDirectory)))
                {
                    if (GUILayout.Button(EditorGUIUtility.TrIconContent("FolderOpened On Icon", "Open the current output folder in the File Explorer. This function is enabled when the directory exist."), Styles.exportButton, GUILayout.MaxWidth(24)))
                    {
                        Application.OpenURL(_map.exportSettings.OutputDirectory);
                    }
                }
                if (GUILayout.Button(EditorGUIUtility.TrIconContent("icon dropdown"), Styles.exportButton, GUILayout.MaxWidth(24)))
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Open Custom Export Folder", "Open custom export folder in the File Explorer. The default folder will be the path in your Unity project/Treasured Data"), false, () =>
                    {
                        if (Directory.Exists(TreasuredSDKPreferences.Instance.customExportFolder))
                        {
                            Application.OpenURL(TreasuredSDKPreferences.Instance.customExportFolder);
                        }
                        else
                        {
                            if(EditorUtility.DisplayDialog("Error", "Custom export folder does not exist.", "Config", "Cancel"))
                            {
                                SettingsService.OpenUserPreferences("Preferences/Treasured SDK");
                            }
                        }
                    });
                    menu.ShowAsContext();
                }
            }
            using(var scope = new EditorGUI.ChangeCheckScope())
            {
                _selectedTabIndex = GUILayout.SelectionGrid(_selectedTabIndex, _tabGroupStates.Select(x => x.attribute.groupName).ToArray(), _tabGroupStates.Length, Styles.TabButton);
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

        [TabGroup(groupName = "Page Info")]
        public void OnPageInfoGUI()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_author"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_title"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_description"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_audioUrl"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_muteOnStart"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_templateLoader"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("headHTML"));
            SerializedProperty uiSettings = serializedObject.FindProperty("uiSettings");
            EditorGUILayout.PropertyField(uiSettings);
            SerializedProperty features = serializedObject.FindProperty("features");
            EditorGUILayout.PropertyField(features);
        }

        [TabGroup(groupName = "Objects")]
        public void OnObjectsGUI()
        {
            EditorGUI.indentLevel++;
            for (int i = 0; i < _foldoutGroupStates.Length; i++)
            {
                using(new GUILayout.VerticalScope(i % 2 == 0 ? Styles.BorderlessBoxEven : Styles.BorderlessBoxOdd))
                {
                    var state = _foldoutGroupStates[i];
                    using (new GUILayout.HorizontalScope())
                    {
                        state.expanded = EditorGUILayout.Foldout(state.expanded, new GUIContent(state.groupName), true);
                        if (GUILayout.Button(EditorGUIUtility.TrIconContent("Toolbar Plus", $"Create new {ObjectNames.NicifyVariableName(state.type.Name)}"), EditorStyles.label, GUILayout.Width(20)))
                        {
                            state.objects.Add((target as TreasuredMap).CreateObject(state.type));
                        }
                    }
                    if (state.expanded)
                    {
                        using (new EditorGUILayout.VerticalScope())
                        {
                            if (state.objects.Count > 0)
                            {
                                using (new EditorGUILayout.HorizontalScope())
                                {
                                    EditorGUILayout.LabelField(new GUIContent(state.type == typeof(Hotspot) ? "Order" : string.Empty, state.type == typeof(Hotspot) ? "The order of the Hotspot for the Guide Tour." : string.Empty), GUILayout.Width(58));
                                    EditorGUILayout.LabelField(new GUIContent("Name"), GUILayout.Width(64));
                                    GUILayout.FlexibleSpace();
                                    if (GUILayout.Button(GUIIcons.menu, EditorStyles.label, GUILayout.Width(18), GUILayout.Height(20)))
                                    {
                                        ShowObjectListMenu(state.objects, state.type);
                                    };
                                }
                                using (new EditorGUILayout.HorizontalScope())
                                {
                                    if (state.objects.All(x => !x.gameObject.activeSelf))
                                    {
                                        state.enableAll = false;
                                        state.toggleState = GroupToggleState.None;
                                    }
                                    else if (state.objects.Any(x => !x.gameObject.activeSelf))
                                    {
                                        state.toggleState = GroupToggleState.Mixed;
                                    }
                                    else
                                    {
                                        state.enableAll = true;
                                        state.toggleState = GroupToggleState.All;
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
                                EditorGUILayout.LabelField($"No {ObjectNames.NicifyVariableName(state.type.Name)} Found", EditorStyles.centeredGreyMiniLabel);
                            }
                            else
                            {
                                using (var scope = new EditorGUILayout.ScrollViewScope(state.scrollPosition, GUILayout.Height(state.objects.Count == 0 ? 20 : Mathf.Clamp(state.objects.Count * 20, state.objects.Count * 20, 200))))
                                {
                                    state.scrollPosition = scope.scrollPosition;
                                    for (int index = 0; index < state.objects.Count; index++)
                                    {
                                        using (new EditorGUILayout.HorizontalScope())
                                        {
                                            TreasuredObject current = state.objects[index];
                                            // TODO: width 40 only show up to 10000
                                            EditorGUI.BeginChangeCheck();
                                            bool active = EditorGUILayout.Toggle(GUIContent.none, current.gameObject.activeSelf, GUILayout.Width(20));
                                            if (EditorGUI.EndChangeCheck())
                                            {
                                                current.gameObject.SetActive(active);
                                            }
                                            EditorGUILayout.LabelField($"{index + 1}", GUILayout.Width(32));
                                            using (var hs = new EditorGUILayout.HorizontalScope())
                                            {
                                                using (new EditorGUI.DisabledGroupScope(!current.gameObject.activeSelf))
                                                {
                                                    EditorGUILayout.LabelField(new GUIContent(current.gameObject.name, current.Id), style: Styles.objectLabel);
                                                }
                                            }
                                            switch (EditorGUILayoutUtils.CreateClickZone(Event.current, GUILayoutUtility.GetLastRect(), MouseCursor.Link))
                                            {
                                                case 0:
                                                    if (current is Hotspot hotspot)
                                                    {
                                                        SceneView.lastActiveSceneView?.LookAt(hotspot.Camera.transform.position, hotspot.Camera.transform.rotation, 0.01f);
                                                    }
                                                    else
                                                    {
                                                        // Always oppsite to the transform.forward
                                                        if (current.Hitbox != null)
                                                        {
                                                            Vector3 targetPosition = current.Hitbox.transform.position;
                                                            Vector3 cameraPosition = current.Hitbox.transform.position + current.Hitbox.transform.forward * 1;
                                                            SceneView.lastActiveSceneView.LookAt(cameraPosition, Quaternion.LookRotation(targetPosition - cameraPosition), 1);
                                                        }
                                                    }
                                                    EditorGUIUtility.PingObject(current);
                                                    break;
                                                case 1:
                                                    GenericMenu menu = new GenericMenu();
#if UNITY_2020_3_OR_NEWER
                                                    menu.AddItem(new GUIContent("Rename"), false, () =>
                                                    {
                                                        GameObjectUtils.RenameGO(current.gameObject);
                                                    });
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

        [TabGroup(groupName = "Export Settings")]
        public void OnExportGUI()
        {
            _exportSettingsEditor.serializedObject.Update();
            _exportSettingsEditor.OnInspectorGUI();
            _exportSettingsEditor.serializedObject.ApplyModifiedProperties();
            try
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Exporter Settings", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(new GUIContent("Overwrite", "Overwrite all exporter settings from Preferences")))
                {
                    foreach (var editor in _exporterEditors)
                    {
                        var source = TreasuredSDKPreferences.Instance.exporters.FirstOrDefault(exporter => exporter.GetType() == editor.target.GetType());
                        if (source == null)
                        {
                            continue;
                        }
                        SerializedObject serializedObject = new SerializedObject(source);
                        SerializedProperty current = serializedObject.GetIterator();
                        while (current.Next(true))
                        {
                            editor.serializedObject.CopyFromSerializedProperty(current);
                        }
                        // set _map for the exporter
                        editor.serializedObject.FindProperty("_map").objectReferenceValue = _map;
                        editor.serializedObject.ApplyModifiedProperties();
                    }
                }
                EditorGUILayout.EndHorizontal();
                using (new EditorGUI.IndentLevelScope(1))
                {
                    for (int i = 0; i < _exporterEditors.Length; i++)
                    {
                        Editor editor = _exporterEditors[i];
                        editor.serializedObject.Update();
                        var exporter = (editor.serializedObject.targetObject as Exporter);
                        EditorGUI.BeginChangeCheck();
                        using (var scope = new ExporterEditor.ExporterScope(exporter))
                        {
                            editor.OnInspectorGUI();
                        }
                        if (EditorGUI.EndChangeCheck())
                        {
                            editor.serializedObject.ApplyModifiedProperties();
                        }
                    }
                }
            }
            catch (ContextException e)
            {
                EditorGUIUtility.PingObject(e.Context);
                Debug.LogException(e);
            }
            catch (TreasuredException e)
            {
                EditorGUIUtility.PingObject(_map);
                Debug.LogException(e);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                UnityEditor.EditorUtility.ClearProgressBar();
            }
        }

        void ShowObjectListMenu(IList<TreasuredObject> objects, Type type)
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(Styles.selectAll, false, () =>
            {
                Selection.objects = _map.GetComponentsInChildren(type).Select(x => x.gameObject).ToArray();
            });
            menu.ShowAsContext();
        }

        private void OnSceneViewGUI(SceneView view)
        {
            if (SceneView.lastActiveSceneView.size == 0.01f) // this happens when TreasuredObject is selected
            {
                return;
            }
            for (int i = 0; i < _hotspots.Count; i++)
            {
                Hotspot current = _hotspots[i];
                if (!current.gameObject.activeSelf)
                {
                    continue;
                }
                Hotspot next = GetNextActiveHotspot(i, _hotspots);

                Transform hitboxTransform = current.Hitbox.transform;
                Transform cameraTransform = current.Camera.transform;

                if (Selection.activeGameObject != current.gameObject)
                {
                    Handles.color = Color.white;
                    Handles.DrawDottedLine(hitboxTransform.position, cameraTransform.position, 5);
                }

                if (!_map.Loop && i == _hotspots.Count - 1)
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
                    Handles.ArrowHandleCap(0, hitboxTransform.position, Quaternion.LookRotation(direction), 0.5f, EventType.Repaint);
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
