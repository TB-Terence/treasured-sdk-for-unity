using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Treasured.UnitySdk.Editor
{
    [CustomEditor(typeof(TreasuredMap))]
    internal class TreasuredMapEditor : UnityEditor.Editor
    {
        private static readonly string[] selectableObjectListNames = new string[] { "Hotspots", "Interactables" };
        private static Vector3 cameraBoxSize = new Vector3(0.3f, 0.3f, 0.3f);

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

        static class Styles
        {
            public static readonly GUIContent alignView = EditorGUIUtility.TrTextContent("Align View");
            public static readonly GUIContent snapAllToGround = EditorGUIUtility.TrTextContent("Snap All on Ground");
            public static readonly GUIContent selectAll = EditorGUIUtility.TrTextContent("Select All");

            public static readonly GUIContent folderOpened = EditorGUIUtility.TrIconContent("FolderOpened Icon", "Show in Explorer");

            public static readonly Dictionary<Type, GUIContent> createNew = new Dictionary<Type, GUIContent>()
            {
                { typeof(Hotspot), EditorGUIUtility.TrTextContent("Create New", "Creaet new Hotspot", "Toolbar Plus") },
                { typeof(Interactable), EditorGUIUtility.TrTextContent("Create New", "Create new Interactable", "Toolbar Plus") }
            };


            public static readonly GUIStyle objectLabel = new GUIStyle("label")
            {
                fontStyle = FontStyle.Bold
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

        private TreasuredMapExporter exporter;

        private SerializedProperty _id;

        private SerializedProperty _title;
        private SerializedProperty _description;

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

        private bool canExport = true;

        private SerializedProperty _outputFolderName;

        private TreasuredMap map;

        private TreasuredObject editingTarget;

        private void OnEnable()
        {
            map = target as TreasuredMap;

            map.transform.hideFlags = HideFlags.None; // should be removed once merge is done

            GetFoldoutGroupMethods();

            _id = serializedObject.FindProperty(nameof(_id));

            _title = serializedObject.FindProperty(nameof(_title));
            _description = serializedObject.FindProperty(nameof(_description));


            _outputFolderName = serializedObject.FindProperty(nameof(_outputFolderName));
            if (string.IsNullOrEmpty(_outputFolderName.stringValue))
            {
                _outputFolderName.stringValue = EditorSceneManager.GetActiveScene().name;
                serializedObject.ApplyModifiedProperties();
            }

            if (map)
            {
                hotspots = map.gameObject.GetComponentsInChildren<Hotspot>().ToList();
                interactables = map.gameObject.GetComponentsInChildren<Interactable>().ToList();

                exporter = new TreasuredMapExporter(serializedObject, map);

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
            }

            SceneView.duringSceneGui -= OnSceneViewGUI;
            SceneView.duringSceneGui += OnSceneViewGUI;
        }

        private void Migrate<T>(List<T> objects) where T : TreasuredObject
        {
            foreach (var to in objects)
            {
                var actionList = to.OnSelected.ToList();
                if (to.ActionGroups.Count == 0 && actionList.Count > 0)
                {
                    ActionGroup group = ScriptableObject.CreateInstance<ActionGroup>();
                    to.ActionGroups.Add(group);
                    foreach (var action in actionList)
                    {
                        group.Actions.Add(action);
                    }
                }
                if (to is Hotspot hotspot)
                {
                    if (hotspot.CameraTransform == null)
                    {
                        hotspot.GroupHotspot();
                    }
                }
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
            if (editingTarget)
            {
                if (editingTarget is Hotspot hotspot)
                {
                    TransformData cameraTransform = hotspot.CameraTransform;
                    var cameraRotation = Quaternion.Euler(cameraTransform.Rotation);
                    switch (Tools.current)
                    {
                        case Tool.Move:
                            EditorGUI.BeginChangeCheck();
                            Vector3 newCameraPosition = Handles.PositionHandle(cameraTransform.Position, cameraRotation);
                            if (EditorGUI.EndChangeCheck())
                            {
                                Undo.RecordObject(editingTarget, "Undo move camera position offset");
                                hotspot.CameraPositionOffset = newCameraPosition - hotspot.transform.position;
                            }
                            break;
                        case Tool.Rotate:
                            EditorGUI.BeginChangeCheck();
                            Quaternion newRotation = Handles.RotationHandle(cameraRotation, cameraTransform.Position);
                            if (EditorGUI.EndChangeCheck())
                            {
                                Undo.RecordObject(editingTarget.transform, "Undo move camera rotation offset");
                                hotspot.CameraRotationOffset = newRotation.eulerAngles - hotspot.transform.eulerAngles;
                            }
                            float size = HandleUtility.GetHandleSize(hotspot.transform.position);
                            Handles.color = Color.blue;
                            Handles.ArrowHandleCap(0, cameraTransform.Position, cameraRotation, size, EventType.Repaint);
                            break;
                    }
                }
                if (Tools.current == Tool.Move)
                {
                    EditorGUI.BeginChangeCheck();
                    Vector3 newPosition = Handles.PositionHandle(editingTarget.transform.position, editingTarget.transform.rotation);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(editingTarget.transform, "Move move");
                        editingTarget.transform.position = newPosition;
                    }
                }
                else if(Tools.current == Tool.Rotate)
                {
                    EditorGUI.BeginChangeCheck();
                    Quaternion newRotation = Handles.RotationHandle(editingTarget.transform.rotation, editingTarget.transform.position);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(editingTarget.transform, "Undo rotate");
                        editingTarget.transform.rotation = newRotation;
                    }
                }
            }
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

                Transform hitboxTransform = current.Transform;
                Transform cameraTransform = current.CameraTransform;

                if (Selection.activeGameObject != current.gameObject)
                {
                    Handles.color = Color.white;
                    Handles.DrawDottedLine(hitboxTransform.position, cameraTransform.position, 5);

                    Handles.color = Color.red;
                    Handles.DrawWireCube(cameraTransform.position, cameraBoxSize);

                    // Show facing direction
                    Handles.color = Color.blue;
                    Handles.ArrowHandleCap(0, cameraTransform.position, cameraTransform.rotation, 0.5f, EventType.Repaint);
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
                Handles.DrawLine(hitboxTransform.position, next.Transform.position);
                Vector3 direction = next.Transform.position - hitboxTransform.position;
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
                    guiMethod.Key.Invoke(this, null);
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
            }
        }

        [FoldoutGroup("Info")]
        void OnInfoGUI()
        {
            EditorGUILayout.PropertyField(_id);
        }

        [FoldoutGroup("Launch Page Settings")]
        void OnLaunchPageSettingsGUI()
        {
            EditorGUILayoutUtilities.RequiredPropertyField(_title);
            EditorGUILayoutUtilities.RequiredPropertyField(_description);
            //EditorGUILayout.PropertyField(cover);
            //if (cover.objectReferenceValue is Texture2D preview)
            //{
            //    Rect previewRect = EditorGUILayout.GetControlRect(false, height: 128);
            //    EditorGUI.DrawPreviewTexture(previewRect, preview, null, ScaleMode.ScaleToFit);
            //}
        }

        [FoldoutGroup("Guide Tour Settings")]
        void OnGuideTourSettingsGUI()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_loop"));
        }

        private int layer;

        [FoldoutGroup("Object Management", true)]
        void OnObjectManagementGUI()
        {
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
                    Application.OpenURL(TreasuredMapExporter.DefaultOutputFolderPath);
                }
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_format"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_quality"));
            using (new EditorGUI.DisabledGroupScope(!canExport))
            {
                if (GUILayout.Button(new GUIContent("Export"), GUILayout.Height(24)))
                {
                    GenericMenu menu = new GenericMenu();
                    foreach (var option in Enum.GetValues(typeof(ExportOptions)))
                    {
                        menu.AddItem(new GUIContent(ObjectNames.NicifyVariableName(option.ToString())), false, () =>
                        {
                            exporter.Export((ExportOptions)option);
                        });
                    }
                    menu.ShowAsContext();
                }
            }
        }

        [FoldoutGroup("Upload", true)]
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
                            if (current.gameObject.activeSelf && EditorGUILayoutUtilities.CreateClickZone(Event.current, GUILayoutUtility.GetLastRect(), MouseCursor.Link, 0))
                            {
                                if (current is Hotspot hotspot)
                                {
                                    SceneView.lastActiveSceneView.LookAt(hotspot.CameraTransform.position, hotspot.CameraTransform.rotation, 0.01f);
                                }
                                else
                                {
                                    // Always oppsite to the transform.forward
                                    Vector3 targetPosition = current.Transform.position;
                                    Vector3 cameraPosition = current.Transform.position + current.Transform.forward * 1;
                                    SceneView.lastActiveSceneView.LookAt(cameraPosition, Quaternion.LookRotation(targetPosition - cameraPosition), 1);
                                }
                                EditorGUIUtility.PingObject(current);

                            }
                        }
                    }
                }
                if (GUILayout.Button(Styles.createNew[typeof(T)]))
                {
                    var root = GetChildOrCreateNew(map.transform, $"{typeof(T).Name}s");
                    GameObject go = new GameObject(ObjectNames.GetUniqueName(objects.Select(x => x.name).ToArray(), typeof(T).Name));
                    T obj = go.AddComponent<T>();
                    BoxCollider boxCollider = go.AddComponent<BoxCollider>();
                    boxCollider.size = Vector3.one;
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
                        go.transform.position = hit.point;
                        boxCollider.center = new Vector3(0, boxCollider.size.y / 2, 0);
                    }
                    else
                    {
                        SceneView.lastActiveSceneView.LookAt(go.transform.position, camera.transform.rotation);
                    }
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

        #region Context Menu
        /// <summary>
        /// Get the child with the name. Create a new game object if child not found.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        static Transform GetChildOrCreateNew(Transform parent, string name)
        {
            Transform child = parent.Find(name);
            if (child == null)
            {
                child = new GameObject(name).transform;
                child.SetParent(parent);
            }
            return child;
        }

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
