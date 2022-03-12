using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [CustomEditor(typeof(TreasuredMap))]
    internal class TreasuredMapEditor : Editor
    {
        [MenuItem("Tools/Treasured/Upgrade to Latest", priority = 99)]
        static void UpgradeToLatest()
        {
            Client.Add("https://github.com/TB-Terence/treasured-sdk-for-unity.git#upm");
        }

        private static readonly string[] selectableObjectListNames = new string[] { "Hotspots", "Interactables", "Videos", "Sounds" };

        [AttributeUsage(AttributeTargets.Method)]
        class FoldoutGroupAttribute : Attribute
        {
            public string Name { get; set; }

            public bool DefaultState { get; set; }

            public FoldoutGroupAttribute(string name)
            {
                Name = name;
            }

            public FoldoutGroupAttribute(string name, bool defaultState) : this(name)
            {
                DefaultState = defaultState;
            }

            public FoldoutGroupAttribute()
            {
            }
        }

        class FoldoutGroupState
        {
            public string name;
            public bool show;

            public FoldoutGroupState(string name, bool show)
            {
                this.name = name;
                this.show = show;
            }
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
            public static readonly GUIContent searchObjects = EditorGUIUtility.TrTextContent("Search", "Search objects by Id or name");

            public static readonly GUIContent folderOpened = EditorGUIUtility.TrIconContent("FolderOpened Icon", "Show in Explorer");

            public static readonly GUIContent search = EditorGUIUtility.TrIconContent("Search Icon");

            public static readonly Dictionary<Type, GUIContent> createNew = new Dictionary<Type, GUIContent>()
            {
                { typeof(Hotspot), EditorGUIUtility.TrTextContent("Create New", "Creaet new Hotspot", "Toolbar Plus") },
                { typeof(Interactable), EditorGUIUtility.TrTextContent("Create New", "Create new Interactable", "Toolbar Plus") },
                { typeof(VideoRenderer), EditorGUIUtility.TrTextContent("Create New", "Create new Video Renderer", "Toolbar Plus") },
                { typeof(SoundSource), EditorGUIUtility.TrTextContent("Create New", "Create new Sound Source", "Toolbar Plus") }
            };


            public static readonly GUIStyle objectLabel = new GUIStyle("label")
            {
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
                            margin = new RectOffset()
                        };
                    }
                    return tabButton;
                }
            }

            private static GUIStyle borderlessBox;
            /// <summary>
            /// Box without margin
            /// </summary>
            public static GUIStyle BorderlessBox
            {
                get
                {
                    if (borderlessBox == null)
                    {
                        borderlessBox = new GUIStyle("box")
                        {
                            margin = new RectOffset()
                        };
                    }
                    return borderlessBox;
                }
            }
        }

        enum GroupToggleState
        {
            None,
            All,
            Mixed
        }

        private static TreasuredMapGizmosSettings gizmosSettings = new TreasuredMapGizmosSettings();

        private SerializedProperty _id;

        private SerializedProperty _author;
        private SerializedProperty _title;
        private SerializedProperty _description;
        private SerializedProperty _audioUrl;
        private SerializedProperty _muteOnStart;
        private SerializedProperty _templateLoader;
        private SerializedProperty headHTML;
        private SerializedProperty uiSettings;

        private bool exportAllHotspots = true;
        private GroupToggleState hotspotsGroupToggleState = GroupToggleState.All;
        private Vector2 hotspotsScrollPosition;

        private bool exportAllInteractables = true;
        private GroupToggleState interactablesGroupToggleState = GroupToggleState.All;
        private Vector2 interactablesScrollPosition;

        private bool exportAllVideos = true;
        private GroupToggleState videosGroupToggleState = GroupToggleState.All;
        private Vector2 videosScrollPosition;

        private bool exportAllSoundSources = true;
        private GroupToggleState soundSourcesGroupToggleState = GroupToggleState.All;
        private Vector2 soundSourcesScrollPosition;

        private int selectedObjectListIndex = 0;

        private List<Hotspot> hotspots = new List<Hotspot>();
        private List<Interactable> interactables = new List<Interactable>();
        private List<VideoRenderer> videos = new List<VideoRenderer>();
        private List<SoundSource> sounds = new List<SoundSource>();

        private Dictionary<MethodInfo, FoldoutGroupState> foldoutGroupGUI = new Dictionary<MethodInfo, FoldoutGroupState>();

        private TreasuredMap map;

        private TreasuredObject editingTarget;

        private string searchString;

        private SerializedProperty _outputFolderName;

        private Editor[] exporterEditors;

        private Editor exportSettingsEditor;

        private void OnEnable()
        {
            map = target as TreasuredMap;
            _id = serializedObject.FindProperty(nameof(_id));
            _author = serializedObject.FindProperty(nameof(_author));
            _title = serializedObject.FindProperty(nameof(_title));
            _description = serializedObject.FindProperty(nameof(_description));
            _audioUrl = serializedObject.FindProperty(nameof(_audioUrl));
            _muteOnStart = serializedObject.FindProperty(nameof(_muteOnStart));
            _templateLoader = serializedObject.FindProperty(nameof(_templateLoader));
            uiSettings = serializedObject.FindProperty(nameof(uiSettings));
            headHTML = serializedObject.FindProperty(nameof(headHTML));
            _outputFolderName = serializedObject.FindProperty(nameof(_outputFolderName));
            ValidateSettings();
            exportSettingsEditor = Editor.CreateEditor(serializedObject.FindProperty(nameof(TreasuredMap.exportSettings)).objectReferenceValue);

            GetFoldoutGroupMethods();

            hotspots = map.gameObject.GetComponentsInChildren<Hotspot>(true).ToList();
            interactables = map.gameObject.GetComponentsInChildren<Interactable>(true).ToList();
            videos = map.gameObject.GetComponentsInChildren<VideoRenderer>(true).ToList();
            sounds = map.gameObject.GetComponentsInChildren<SoundSource>(true).ToList();

            serializedObject.FindProperty("_format").enumValueIndex = 2;
            serializedObject.ApplyModifiedProperties();

            // Set icon for hotspots
            foreach (var hotspot in hotspots)
            {
                if (hotspot.gameObject.GetIcon() == null)
                {
                    hotspot.gameObject.SetLabelIcon(6);
                }
            }

            // Migrate output folder
            if (string.IsNullOrEmpty(map.exportSettings.folderName))
            {
                if (string.IsNullOrEmpty( this._outputFolderName.stringValue))
                {
                    map.exportSettings.folderName = EditorSceneManager.GetActiveScene().name;
                }
                else
                {
                    map.exportSettings.folderName = this._outputFolderName.stringValue;
                }
            }

            SceneView.duringSceneGui -= OnSceneViewGUI;
            SceneView.duringSceneGui += OnSceneViewGUI;
        }

        private void ValidateSettings()
        {
            SerializedProperty exportSettings = serializedObject.FindProperty(nameof(TreasuredMap.exportSettings));
            if (exportSettings.objectReferenceValue == null)
            {
                exportSettings.objectReferenceValue = ScriptableObject.CreateInstance<ExportSettings>();
            }
            // Find all serialized export processes
            FieldInfo[] exportProcessFields = typeof(TreasuredMap).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).
                Where(x => 
                typeof(Exporter).IsAssignableFrom(x.FieldType) && 
                (x.IsPublic || (x.IsPrivate && x.IsDefined(typeof(SerializeField))))).ToArray();
            exporterEditors = new Editor[exportProcessFields.Length];
            for (int i = 0; i < exportProcessFields.Length; i++)
            {
                FieldInfo fi = exportProcessFields[i];
                SerializedProperty exportProcess = serializedObject.FindProperty(fi.Name);
                if (exportProcess.objectReferenceValue == null)
                {
                    exportProcess.objectReferenceValue = ScriptableObject.CreateInstance(fi.FieldType);
                }
                SerializedObject so = new SerializedObject(exportProcess.objectReferenceValue);
                SerializedProperty map = so.FindProperty("_map");
                if(map.objectReferenceValue == null)
                {
                    map.objectReferenceValue = target;
                }
                so.ApplyModifiedProperties();
                exporterEditors[i] = CreateEditor(exportProcess.objectReferenceValue);
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneViewGUI;
        }

        private void GetFoldoutGroupMethods()
        {
            foldoutGroupGUI.Clear();
            var methods = typeof(TreasuredMapEditor).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).Where(x => x.GetParameters().Length == 0);
            foreach (var method in methods)
            {
                var attribute = method.GetCustomAttribute<FoldoutGroupAttribute>();
                if (attribute != null)
                {
                    foldoutGroupGUI[method] = new FoldoutGroupState(attribute.Name, attribute.DefaultState);
                }
            }
        }


        private void OnSceneViewGUI(SceneView view)
        {
            if (SceneView.lastActiveSceneView.size == 0.01f) // this happens when TreasuredObject is selected
            {
                return;
            }
            for (int i = 0; i < hotspots.Count; i++)
            {
                Hotspot current = hotspots[i];
                if (!current.gameObject.activeSelf)
                {
                    continue;
                }
                Hotspot next = GetNextActiveHotspot(i, hotspots);

                Transform hitboxTransform = current.Hitbox.transform;
                Transform cameraTransform = current.Camera.transform;

                if (Selection.activeGameObject != current.gameObject)
                {
                    Handles.color = Color.white;
                    Handles.DrawDottedLine(hitboxTransform.position, cameraTransform.position, 5);
                }

                if (!map.Loop && i == hotspots.Count - 1)
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

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            OnFoldoutGroupGUI();
            serializedObject.ApplyModifiedProperties();
        }

        void OnFoldoutGroupGUI()
        {
            foreach (var guiMethod in foldoutGroupGUI)
            {
                var state = foldoutGroupGUI[guiMethod.Key];
                state.show = EditorGUILayout.BeginFoldoutHeaderGroup(state.show, state.name);
                if (state.show)
                {
                    EditorGUI.indentLevel++;
                    guiMethod.Key.Invoke(this, null);
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
            }
        }

        static void OnSelectTreasuredObject()
        {
            var to = Selection.activeGameObject?.GetComponent<TreasuredObject>();
            if (to && TreasuredSDKSettingsProvider.Settings.autoFocus)
            {
                to.TryInvokeMethods("OnSceneViewFocus");
            }
        }

        [FoldoutGroup("Page Info")]
        void OnLandingPagePropertiesGUI()
        {
            EditorGUILayoutHelper.RequiredPropertyField(_author);
            EditorGUILayoutHelper.RequiredPropertyField(_title);
            EditorGUILayoutHelper.RequiredPropertyField(_description);
            EditorGUILayout.PropertyField(_audioUrl);
            EditorGUILayout.PropertyField(_muteOnStart);
            EditorGUILayout.PropertyField(_templateLoader);
            EditorGUILayout.PropertyField(headHTML);
            EditorGUILayout.PropertyField(uiSettings.FindPropertyRelative("projectDomeOntoGeometry"));
            EditorGUILayout.PropertyField(uiSettings.FindPropertyRelative("showOnboarding"));
        }

        [FoldoutGroup("Object Management", true)]
        void OnObjectManagementGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                searchString = EditorGUILayout.TextField(Styles.searchObjects, searchString);
                if (GUILayout.Button(Styles.search, EditorStyles.label, GUILayout.Width(18), GUILayout.Height(18)))
                {
                    List<GameObject> searchResult = new List<GameObject>();
                    foreach (var obj in map.GetComponentsInChildren<TreasuredObject>(true))
                    {
                        if (obj.Id.Contains(searchString) || obj.name.Contains(searchString))
                        {
                            searchResult.Add(obj.gameObject);
                        }
                    }
                    Selection.objects = searchResult.ToArray();
                }
            }
            selectedObjectListIndex = GUILayout.SelectionGrid(selectedObjectListIndex, selectableObjectListNames, selectableObjectListNames.Length, Styles.TabButton);
            if (selectedObjectListIndex == 0)
            {
                OnObjectList(hotspots, ref hotspotsScrollPosition, ref exportAllHotspots, ref hotspotsGroupToggleState);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_loop"));
                EditorGUILayout.PropertyField(uiSettings.FindPropertyRelative("showHotspotButtons"));
            }
            else if (selectedObjectListIndex == 1)
            {
                OnObjectList(interactables, ref interactablesScrollPosition, ref exportAllInteractables, ref interactablesGroupToggleState);
                EditorGUILayout.PropertyField(uiSettings.FindPropertyRelative("showInteractableButtons"));
            }
            else if (selectedObjectListIndex == 2)
            {
                OnObjectList(videos, ref videosScrollPosition, ref exportAllVideos, ref videosGroupToggleState);
            }
            else if (selectedObjectListIndex == 3)
            {
                OnObjectList(sounds, ref soundSourcesScrollPosition, ref exportAllSoundSources, ref soundSourcesGroupToggleState);
            }
        }

        [FoldoutGroup("Gizmos", true)]
        void OnGizmosGUI()
        {
            EditorGUILayout.LabelField(new GUIContent("Editor"), EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUI.BeginChangeCheck();
                gizmosSettings.enableCameraPreview = EditorGUILayout.Toggle(new GUIContent("Camera Preview", "Show the camera preview for when selection changed in the hierarchy."), gizmosSettings.enableCameraPreview);
                if (EditorGUI.EndChangeCheck())
                {
                    if (gizmosSettings.enableCameraPreview)
                    {
                        Selection.selectionChanged -= OnSelectTreasuredObject;
                        Selection.selectionChanged += OnSelectTreasuredObject;
                    }
                    else
                    {
                        Selection.selectionChanged -= OnSelectTreasuredObject;
                    }
                }
            }
        }

        [FoldoutGroup("Export", true)]
        void OnExportGUI()
        {
            exportSettingsEditor.OnInspectorGUI();
            try
            {
                for (int i = 0; i < exporterEditors.Length; i++)
                {
                    Editor editor = exporterEditors[i];
                    SerializedProperty enabled = editor.serializedObject.FindProperty(nameof(Exporter.enabled));
                    enabled.boolValue = EditorGUILayout.ToggleLeft(ObjectNames.NicifyVariableName(editor.target.GetType().Name), enabled.boolValue, EditorStyles.boldLabel);
                    EditorGUI.indentLevel++;
                    editor.serializedObject.Update();
                    editor.OnInspectorGUI();
                    editor.serializedObject.ApplyModifiedProperties();
                    EditorGUI.indentLevel--;
                }
                if (GUILayout.Button(new GUIContent("Export", "Export all enabled process."), GUILayout.Height(24)))
                {
                    DataValidator.ValidateMap(map);
                    if (Directory.Exists(map.exportSettings.OutputDirectory))
                    {
                        Directory.Delete(map.exportSettings.OutputDirectory, true);
                    }
                    Directory.CreateDirectory(map.exportSettings.OutputDirectory); // try create the directory if not exist.
                    foreach (var editor in exporterEditors)
                    {
                        Exporter process = (Exporter)editor.target;
                        if (process != null && process.enabled)
                        {
                            process.OnPreExport();
                            process.Export();
                            process.OnPostExport();
                        }
                    }
                }
            }
            catch (ContextException e)
            {
                if (EditorUtility.DisplayDialog(e.Title, e.Message, e.PingText))
                {
                    EditorGUIUtility.PingObject(e.Context);
                }
                Debug.LogException(e);
            }
            catch (TreasuredException e)
            {
                EditorUtility.DisplayDialog(e.Title, e.Message, "Ok");
                Debug.LogException(e);
            }
            catch (Exception e)
            {
                string exceptionType = e.GetType().Name.ToString();
                if (exceptionType.EndsWith("Exception"))
                {
                    exceptionType = exceptionType.Substring(0, exceptionType.LastIndexOf("Exception"));
                }
                EditorUtility.DisplayDialog(ObjectNames.NicifyVariableName(exceptionType), e.Message, "Ok");
                Debug.LogException(e);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        void OnObjectList<T>(IList<T> objects, ref Vector2 scrollPosition, ref bool enableAll, ref GroupToggleState groupToggleState) where T : TreasuredObject
        {
            using (new EditorGUILayout.VerticalScope(Styles.BorderlessBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(new GUIContent("Index", typeof(T) == typeof(Hotspot) ? "The order of the Hotspot for the Guide Tour." : string.Empty), GUILayout.Width(74));
                    EditorGUILayout.LabelField(new GUIContent("Name"), GUILayout.Width(64));
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(Icons.menu, EditorStyles.label, GUILayout.Width(18), GUILayout.Height(20)))
                    {
                        ShowObjectsMenu(objects);
                    };
                }
                if (objects.Count > 1)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (objects.All(x => !x.gameObject.activeSelf))
                        {
                            enableAll = false;
                            groupToggleState = GroupToggleState.None;
                        }
                        else if (objects.Any(x => !x.gameObject.activeSelf))
                        {
                            groupToggleState = GroupToggleState.Mixed;
                        }
                        else
                        {
                            enableAll = true;
                            groupToggleState = GroupToggleState.All;
                        }
                        EditorGUI.showMixedValue = groupToggleState == GroupToggleState.Mixed;
                        GUILayout.Space(3);
                        EditorGUI.BeginChangeCheck();
                        enableAll = EditorGUILayout.ToggleLeft(GUIContent.none, enableAll);
                        if (EditorGUI.EndChangeCheck())
                        {
                            foreach (var obj in objects)
                            {
                                obj.gameObject.SetActive(enableAll);
                            }
                        }
                        EditorGUI.showMixedValue = false;
                    }
                }
                using (var scope = new EditorGUILayout.ScrollViewScope(scrollPosition, GUILayout.Height(300)))
                {
                    scrollPosition = scope.scrollPosition;
                    for (int i = 0; i < objects.Count; i++)
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            T current = objects[i];
                            // TODO: width 40 only show up to 10000
                            EditorGUI.BeginChangeCheck();
                            bool active = EditorGUILayout.Toggle(GUIContent.none, current.gameObject.activeSelf, GUILayout.Width(20));
                            if (EditorGUI.EndChangeCheck())
                            {
                                current.gameObject.SetActive(active);
                            }
                            EditorGUILayout.LabelField($"{i + 1}", GUILayout.Width(48));
                            using (var hs = new EditorGUILayout.HorizontalScope())
                            {
                                using (new EditorGUI.DisabledGroupScope(!current.gameObject.activeSelf))
                                {
                                    EditorGUILayout.LabelField(new GUIContent(current.gameObject.name, current.Id), style: Styles.objectLabel);
                                }
                            }
                            if (EditorGUILayoutHelper.CreateClickZone(Event.current, GUILayoutUtility.GetLastRect(), MouseCursor.Link, 0))
                            {
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
                            }
                        }
                    }
                }
                if (GUILayout.Button(Styles.createNew[typeof(T)]))
                {
                    var obj = map.CreateObject<T>();
                    objects.Add(obj);
                    editingTarget = obj;
                }
            }
        }

        void ShowObjectsMenu<T>(IList<T> objects) where T : TreasuredObject
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(Styles.selectAll, false, () =>
            {
                Selection.objects = map.GetComponentsInChildren<T>().Select(x => x.gameObject).ToArray();
            });
            menu.ShowAsContext();
        }

        private Hotspot GetNextActiveHotspot(int currentIndex, IList<Hotspot> list)
        {
            int index = currentIndex;
            Hotspot current = list[index];
            Hotspot next = list[(index + 1) % list.Count];
            while (next != current)
            {
                if (index == list.Count - 1 && !map.Loop)
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
