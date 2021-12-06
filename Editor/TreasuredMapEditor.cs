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
    internal class TreasuredMapEditor : UnityEditor.Editor
    {
        [MenuItem("Tools/Treasured/Upgrade to Latest", priority = 99)]
        static void UpgradeToLatest()
        {
            Client.Add("https://github.com/TB-Terence/treasured-sdk-for-unity.git#upm");
        }

        private static readonly string[] selectableObjectListNames = new string[] { "Hotspots", "Interactables" };

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
                { typeof(Interactable), EditorGUIUtility.TrTextContent("Create New", "Create new Interactable", "Toolbar Plus") }
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

        private bool exportAllHotspots = true;
        private GroupToggleState hotspotsGroupToggleState = GroupToggleState.All;
        private Vector2 hotspotsScrollPosition;

        private bool exportAllInteractables = true;
        private GroupToggleState interactablesGroupToggleState = GroupToggleState.All;
        private Vector2 interactablesScrollPosition;

        private int selectedObjectListIndex = 0;

        private List<Hotspot> hotspots = new List<Hotspot>();
        private List<Interactable> interactables = new List<Interactable>();

        private Dictionary<MethodInfo, FoldoutGroupState> foldoutGroupGUI = new Dictionary<MethodInfo, FoldoutGroupState>();

        private TreasuredMap map;

        private TreasuredObject editingTarget;

        private string searchString;

        private ExportProcess[] exportProcesses;


        private SerializedProperty _outputFolderName;

        private bool _overwriteExistingData;

        private void OnEnable()
        {
            map = target as TreasuredMap;

            GetFoldoutGroupMethods();

            _id = serializedObject.FindProperty(nameof(_id));

            _author = serializedObject.FindProperty(nameof(_author));
            _title = serializedObject.FindProperty(nameof(_title));
            _description = serializedObject.FindProperty(nameof(_description));
            _audioUrl = serializedObject.FindProperty(nameof(_audioUrl));
            _muteOnStart = serializedObject.FindProperty(nameof(_muteOnStart));
            _templateLoader = serializedObject.FindProperty(nameof(_templateLoader));
            

            if (map)
            {
                hotspots = map.gameObject.GetComponentsInChildren<Hotspot>().ToList();
                interactables = map.gameObject.GetComponentsInChildren<Interactable>().ToList();

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

                Migrate(hotspots);
                Migrate(interactables);

                this._outputFolderName = serializedObject.FindProperty(nameof(_outputFolderName));
                if (string.IsNullOrEmpty(_outputFolderName.stringValue))
                {
                    _outputFolderName.stringValue = EditorSceneManager.GetActiveScene().name;
                    serializedObject.ApplyModifiedProperties();
                }

                if (exportProcesses == null)
                {
                    var exportProcessTypes = Assembly.GetExecutingAssembly().GetTypes().Where(x => !x.IsAbstract && typeof(ExportProcess).IsAssignableFrom(x)).ToArray();
                    if(exportProcessTypes.Length > 0)
                    {
                        exportProcesses = new ExportProcess[exportProcessTypes.Length];
                        for (int i = 0; i < exportProcesses.Length; i++)
                        {
                            exportProcesses[i] = (ExportProcess)Activator.CreateInstance(exportProcessTypes[i]);
                        }
                    }
                    else
                    {
                        exportProcesses = new ExportProcess[0];
                    }
                }
                foreach (var process in exportProcesses)
                {
                    process.OnEnable(serializedObject);
                }
            }

            SceneView.duringSceneGui -= OnSceneViewGUI;
            SceneView.duringSceneGui += OnSceneViewGUI;
        }

        private void Migrate<T>(List<T> objects) where T : TreasuredObject
        {
            foreach (var to in objects)
            {
                var actionList = to.OnSelected.ToList();
                if (to.OnClick.Count == 0 && actionList.Count > 0)
                {
                    ActionGroup group = ScriptableObject.CreateInstance<ActionGroup>();
                    to.OnClick.Add(group);
                    foreach (var action in actionList)
                    {
                        group.Actions.Add(action);
                    }
                }
                to.TryInvokeMethods("OnSelectedInHierarchy");
            }
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
            if (to && TreasuredSDKSettings.Instance.autoFocus)
            {
                to.TryInvokeMethods("OnSceneViewFocus");
            }
        }

        [FoldoutGroup("Page Info")]
        void OnLaunchPageSettingsGUI()
        {
            EditorGUILayoutHelper.RequiredPropertyField(_author);
            EditorGUILayoutHelper.RequiredPropertyField(_title);
            EditorGUILayoutHelper.RequiredPropertyField(_description);
            EditorGUILayout.PropertyField(_audioUrl);
            EditorGUILayout.PropertyField(_muteOnStart);
            EditorGUILayout.PropertyField(_templateLoader);
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
            SerializedProperty interactableLayer = serializedObject.FindProperty("_interactableLayer");
            EditorGUI.BeginChangeCheck();
            interactableLayer.intValue = EditorGUILayout.LayerField(new GUIContent("Interactable Layer"), interactableLayer.intValue);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (var renderer in map.GetComponentsInChildren<Renderer>())
                {
                    renderer.gameObject.layer = interactableLayer.intValue;
                }
            }
            selectedObjectListIndex = GUILayout.SelectionGrid(selectedObjectListIndex, selectableObjectListNames, selectableObjectListNames.Length, Styles.TabButton);
            if (selectedObjectListIndex == 0)
            {
                OnObjectList(hotspots, ref hotspotsScrollPosition, ref exportAllHotspots, ref hotspotsGroupToggleState);
            }
            else if (selectedObjectListIndex == 1)
            {
                OnObjectList(interactables, ref interactablesScrollPosition, ref exportAllInteractables, ref interactablesGroupToggleState);
            }
        }

        [FoldoutGroup("Gizmos", true)]
        void OnGizmosGUI()
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

        [FoldoutGroup("Export", true)]
        void OnExportGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUI.BeginChangeCheck();
                string newOutputFolderName = EditorGUILayout.TextField(new GUIContent("Output Folder Name"), _outputFolderName.stringValue);
                if (EditorGUI.EndChangeCheck() && !string.IsNullOrWhiteSpace(newOutputFolderName))
                {
                    _outputFolderName.stringValue = newOutputFolderName;
                }
                if (GUILayout.Button(Styles.folderOpened, EditorStyles.label, GUILayout.Width(20), GUILayout.Height(18)))
                {
                    Application.OpenURL(ExportProcess.DefaultOutputFolderPath);
                }
            }
            EditorGUILayout.LabelField("Export Options", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            try
            {
                foreach (var process in exportProcesses)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        float previousLabelWidth = EditorGUIUtility.labelWidth;
                        EditorGUIUtility.labelWidth = 0;
                        process.Enabled = EditorGUILayout.Toggle(GUIContent.none, process.Enabled, GUILayout.Width(24));
                        EditorGUIUtility.labelWidth = previousLabelWidth;
                        process.Expanded = EditorGUILayout.Foldout(process.Expanded, process.DisplayName, true);
                    }
                    if (process.Expanded)
                    {
                        EditorGUI.indentLevel++;
                        process.OnGUI(serializedObject);
                        EditorGUI.indentLevel--;
                    }
                }
                if (GUILayout.Button(new GUIContent("Export", "Export all enabled process."), GUILayout.Height(24)))
                {
                    DataValidator.ValidateMap(map);
                    string root = string.Empty;
                    try
                    {
                        root = Path.Combine(ExportProcess.DefaultOutputFolderPath, (target as TreasuredMap).OutputFolderName).Replace('/', '\\');
                    }
                    catch (Exception ex) when (ex is IOException || ex is ArgumentException)
                    {
                        throw new ArgumentException($"Invalid folder name : {(target as TreasuredMap).OutputFolderName}");
                    }
                    catch
                    {
                        throw;
                    }
                    if (Directory.Exists(root))
                    {
                        Directory.Delete(root, true);
                    }
                    Directory.CreateDirectory(root); // try create the directory if not exist.
                    foreach (var process in exportProcesses)
                    {
                        if (process.Enabled)
                        {
                            process.Export(root, target as TreasuredMap);
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
            }
            catch (TreasuredException e)
            {
                EditorUtility.DisplayDialog(e.Title, e.Message, "Ok");
            }
            catch (Exception e)
            {
                string exceptionType = e.GetType().Name.ToString();
                if (exceptionType.EndsWith("Exception"))
                {
                    exceptionType = exceptionType.Substring(0, exceptionType.LastIndexOf("Exception"));
                }
                EditorUtility.DisplayDialog(ObjectNames.NicifyVariableName(exceptionType), e.Message, "Ok");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
            EditorGUI.indentLevel--;
        }

        //[FoldoutGroup("Upload", true)]
        void OnUploadGUI()
        {
            if (GUILayout.Button("Upload", GUILayout.Height(24)))
            {
                UploadWindow.ShowUploadWindow();
            }
        }

        void OnObjectList<T>(IList<T> objects, ref Vector2 scrollPosition, ref bool exportAll, ref GroupToggleState groupToggleState) where T : TreasuredObject
        {
            using (new EditorGUILayout.VerticalScope(Styles.BorderlessBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(new GUIContent("Index", typeof(T) == typeof(Hotspot) ? "The order of the Hotspot for the Guide Tour." : string.Empty), GUILayout.Width(40));
                    EditorGUILayout.LabelField(new GUIContent("Name"), GUILayout.Width(64));
                    //EditorGUILayout.LabelField(new GUIContent("Export", "Enable if the object should be included in the output file."), GUILayout.Width(72));
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(Icons.menu, EditorStyles.label, GUILayout.Width(18), GUILayout.Height(20)))
                    {
                        ShowObjectsMenu(objects);
                    };
                }
                //if (objects.Count > 1)
                //{
                //    using (new EditorGUILayout.HorizontalScope())
                //    {
                //        if (objects.All(x => !x.gameObject.activeSelf))
                //        {
                //            exportAll = false;
                //            groupToggleState = GroupToggleState.None;
                //        }
                //        else if (objects.Any(x => !x.gameObject.activeSelf))
                //        {
                //            groupToggleState = GroupToggleState.Mixed;
                //        }
                //        else
                //        {
                //            exportAll = true;
                //            groupToggleState = GroupToggleState.All;
                //        }
                //        EditorGUI.showMixedValue = groupToggleState == GroupToggleState.Mixed;
                //        GUILayout.Space(70);
                //        EditorGUI.BeginChangeCheck();
                //        exportAll = EditorGUILayout.ToggleLeft(GUIContent.none, exportAll);
                //        if (EditorGUI.EndChangeCheck())
                //        {
                //            foreach (var obj in objects)
                //            {
                //                obj.gameObject.SetActive(exportAll);
                //            }
                //        }
                //        EditorGUI.showMixedValue = false;
                //    }
                //}
                using (var scope = new EditorGUILayout.ScrollViewScope(scrollPosition, GUILayout.Height(300)))
                {
                    scrollPosition = scope.scrollPosition;
                    for (int i = 0; i < objects.Count; i++)
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            T current = objects[i];
                            // TODO: width 40 only show up to 10000
                            EditorGUILayout.LabelField($"{i + 1}", GUILayout.Width(40));
                            //EditorGUI.BeginChangeCheck();
                            //bool active = EditorGUILayout.Toggle(GUIContent.none, current.gameObject.activeSelf, GUILayout.Width(20));
                            //if (EditorGUI.EndChangeCheck())
                            //{
                            //    current.gameObject.SetActive(active);
                            //}
                            using (var hs = new EditorGUILayout.HorizontalScope())
                            {
                                using (new EditorGUI.DisabledGroupScope(!current.gameObject.activeSelf))
                                {
                                    EditorGUILayout.LabelField(new GUIContent(current.gameObject.name), style: Styles.objectLabel);
                                }
                            }
                            if (current.gameObject.activeSelf && EditorGUILayoutHelper.CreateClickZone(Event.current, GUILayoutUtility.GetLastRect(), MouseCursor.Link, 0))
                            {
                                if (current is Hotspot hotspot)
                                {
                                    SceneView.lastActiveSceneView.LookAt(hotspot.Camera.transform.position, hotspot.Camera.transform.rotation, 0.01f);
                                }
                                else
                                {
                                    // Always oppsite to the transform.forward
                                    Vector3 targetPosition = current.Hitbox.transform.position;
                                    Vector3 cameraPosition = current.Hitbox.transform.position + current.Hitbox.transform.forward * 1;
                                    SceneView.lastActiveSceneView.LookAt(cameraPosition, Quaternion.LookRotation(targetPosition - cameraPosition), 1);
                                }
                                EditorGUIUtility.PingObject(current);
                            }
                        }
                    }
                }
                if (GUILayout.Button(Styles.createNew[typeof(T)]))
                {
                    var root = map.gameObject.FindOrCreateChild($"{typeof(T).Name}s");
                    GameObject go = new GameObject(ObjectNames.GetUniqueName(objects.Select(x => x.name).ToArray(), typeof(T).Name));
                    T obj = go.AddComponent<T>();
                    Camera camera = SceneView.lastActiveSceneView.camera;
                    go.transform.SetParent(root);
                    if (typeof(T) == typeof(Hotspot))
                    {
                        hotspots.Add(obj as Hotspot);
                    }
                    else if (typeof(T) == typeof(Interactable))
                    {
                        interactables.Add(obj as Interactable);
                    }
                    EditorGUIUtility.PingObject(go);
                    if (Physics.Raycast(camera.transform.position, camera.transform.forward, out var hit))
                    {
                        obj.TryInvokeMethods("OnSelectedInHierarchy");
                        obj.transform.position = hit.point;
                        if (obj is Hotspot hotspot)
                        {
                            hotspot.Camera.transform.position = hit.point + new Vector3(0, 1.5f, 0);
                            hotspot.Camera.transform.localRotation = Quaternion.identity;
                        }
                    }
                    else
                    {
                        SceneView.lastActiveSceneView.LookAt(go.transform.position, camera.transform.rotation);
                    }
                    editingTarget = obj;
                }
                if (typeof(T).Equals(typeof(Hotspot)))
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_loop"));
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

        #region Context Menu
        /// <summary>
        /// Get the child with the name. Create a new game object if child not found.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <returns></returns>

        static T CreateTreasuredObject<T>(Transform parent) where T : TreasuredObject
        {
            GameObject go = new GameObject();
            go.transform.SetParent(parent);
            return go.AddComponent<T>();
        }

        [MenuItem("GameObject/Treasured/Create Interactable", false, 49)]
        static void CreateInteractableFromContextMenu()
        {
            TreasuredMap map = Selection.activeGameObject.GetComponentInParent<TreasuredMap>();
            Transform root = map.transform;
            Transform interactableRoot = root.Find("Interactables");
            if (interactableRoot == null)
            {
                interactableRoot = new GameObject("Interactables").transform;
                interactableRoot.SetParent(root);
            }
            GameObject interactable = new GameObject("New Interacatble", typeof(Interactable));
            if (Selection.activeGameObject.transform == root)
            {
                interactable.transform.SetParent(interactableRoot);
            }
            else
            {
                interactable.transform.SetParent(Selection.activeGameObject.transform);
            }
        }

        [MenuItem("GameObject/Treasured/Create Hotspot", false, 49)]
        static void CreateHotspotFromContextMenu()
        {
            TreasuredMap map = Selection.activeGameObject.GetComponentInParent<TreasuredMap>();
            Transform root = map.transform;
            Transform hotspotRoot = root.Find("Hotspots");
            if (hotspotRoot == null)
            {
                hotspotRoot = new GameObject("Hotspots").transform;
                hotspotRoot.SetParent(root);
            }
            GameObject hotspot = new GameObject("New Hotspot", typeof(Hotspot));
            if (Selection.activeGameObject.transform == root)
            {
                hotspot.transform.SetParent(hotspotRoot);
            }
            else
            {
                hotspot.transform.SetParent(Selection.activeGameObject.transform);
            }
        }

        [MenuItem("GameObject/Treasured/Create Empty Map", false, 49)]
        static void CreateEmptyMap()
        {
            GameObject map = new GameObject("Treasured Map", typeof(TreasuredMap));
            if (Selection.activeGameObject)
            {
                map.transform.SetParent(Selection.activeGameObject.transform);
            }
        }

        [MenuItem("GameObject/Treasured/Create Empty Map", true, 49)]
        static bool CanCreateEmptyMap()
        {
            if (Selection.activeGameObject == null)
            {
                return true;
            }
            return !Selection.activeGameObject.GetComponentInParent<TreasuredMap>();
        }

        [MenuItem("GameObject/Treasured/Create Hotspot", true)]
        static bool CanCreateHotspotFromContextMenu()
        {
            return Selection.activeGameObject?.GetComponentInParent<TreasuredMap>();
        }

        [MenuItem("GameObject/Treasured/Create Interactable", true, 49)]
        static bool CanCreateInteractableFromContextMenu()
        {
            return Selection.activeGameObject?.GetComponentInParent<TreasuredMap>();
        }
        #endregion
    }
}
