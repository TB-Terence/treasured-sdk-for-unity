using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
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

        private SerializedProperty _interactableLayer;

        #region Hotspot Management
        #endregion

        #region Version 0.5
        private TreasuredMapExporter exporter;

        private SerializedProperty _title;
        private SerializedProperty _description;

        private SerializedProperty _format;
        private SerializedProperty _quality;

        private SerializedProperty _loop;

        private bool exportAllHotspots = true;
        private GroupToggleState hotspotsGroupToggleState = GroupToggleState.All;
        private Vector2 hotspotsScrollPosition;

        private bool exportAllInteractables = true;
        private GroupToggleState interactablesGroupToggleState = GroupToggleState.All;
        private Vector2 interactablesScrollPosition;

        private int selectedObjectListIndex = 0;

        //private TreasuredMapExporter exporter;

        private List<Hotspot> hotspots = new List<Hotspot>();
        private List<Interactable> interactables = new List<Interactable>();

        private Dictionary<MethodInfo, FoldoutGroupState> foldoutGroupGUI = new Dictionary<MethodInfo, FoldoutGroupState>();
        #endregion

        private void OnEnable()
        {
            GetFoldoutGroupMethods();
            _interactableLayer = serializedObject.FindProperty(nameof(_interactableLayer));

            _title = serializedObject.FindProperty(nameof(_title));
            _description = serializedObject.FindProperty(nameof(_description));

            _loop = serializedObject.FindProperty(nameof(_loop));

            _format = serializedObject.FindProperty(nameof(_format));
            _quality = serializedObject.FindProperty(nameof(_quality));

            TreasuredMap map = (target as TreasuredMap);
            if (map)
            {
                hotspots = map.gameObject.GetComponentsInChildren<Hotspot>(true).ToList();
                interactables = map.gameObject.GetComponentsInChildren<Interactable>(true).ToList();

                exporter = new TreasuredMapExporter(serializedObject, map);
            }
            
            SceneView.duringSceneGui -= OnSceneViewGUI;
            SceneView.duringSceneGui += OnSceneViewGUI;
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

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneViewGUI;
        }

        private void OnSceneViewGUI(SceneView view)
        {
            if (SceneView.lastActiveSceneView.size <= 0.01f)
            {
                return;
            }
            Color previousColor = Handles.color;
            for (int i = 0; i < hotspots.Count; i++)
            {
                Hotspot current = hotspots[i];
                Hotspot next = hotspots[(i + 1) % hotspots.Count];

                Vector3 currentCameraPosition = current.transform.position + current.CameraPositionOffset;
                Vector3 nextCameraPosition = next.transform.position + next.CameraPositionOffset;

                if (Selection.activeGameObject != current.gameObject)
                {
                    Handles.color = Color.white;
                    Handles.Label(currentCameraPosition, current.name);

                    // Show facing direction
                    Handles.color = Color.blue;
                    Handles.ArrowHandleCap(0, currentCameraPosition, current.gameObject.transform.rotation, 0.5f, EventType.Repaint);

                    Handles.color = Color.red;
                    Handles.DrawWireCube(currentCameraPosition, cameraBoxSize);
                }

                if (!_loop.boolValue && i == hotspots.Count - 1)
                {
                    continue;
                }
                Handles.color = Color.white;
                Handles.DrawLine(currentCameraPosition, nextCameraPosition);
                Vector3 direction = nextCameraPosition - currentCameraPosition;
                if (direction != Vector3.zero)
                {
                    Handles.color = Color.green;
                    Handles.ArrowHandleCap(0, currentCameraPosition, Quaternion.LookRotation(direction), 0.5f, EventType.Repaint);
                }
            }
            Handles.color = previousColor;
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

        [FoldoutGroup("Launch Page Settings")]
        void OnLaunchPageSettingsGUI()
        {
            EditorGUILayout.PropertyField(_title);
            EditorGUILayout.PropertyField(_description);
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
            EditorGUILayout.PropertyField(_loop);
        }

        [FoldoutGroup("Object Management", true)]
        void OnObjectManagementGUI()
        {
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
            exporter?.OnGUI();
            if (GUILayout.Button("Open Upload URL", GUILayout.Height(24f)))
            {
                Application.OpenURL("https://dev.world.treasured.ca/upload");
            }
        }
        
        void OnObjectList<T>(IList<T> objects, ref Vector2 scrollPosition, ref bool exportAll, ref GroupToggleState groupToggleState) where T : TreasuredObject
        {
            using (new EditorGUILayout.VerticalScope(Styles.BorderlessBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(new GUIContent("Index", typeof(T) == typeof(Hotspot) ? "The order of the Hotspot for the Guide Tour." : string.Empty), GUILayout.Width(64));
                    //EditorGUILayout.LabelField(new GUIContent("Export", "Enable if the object should be included in the output file."), GUILayout.Width(72));
                    //GUILayout.FlexibleSpace();
                    //if (GUILayout.Button(Icons.menu, EditorStyles.label, GUILayout.Width(20), GUILayout.Height(20)))
                    //{
                    //    ShowObjectsMenu(objects);
                    //};
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
                using (var scope = new EditorGUILayout.ScrollViewScope(scrollPosition, GUILayout.Height(200)))
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
                            using (new EditorGUI.DisabledGroupScope(!current.gameObject.activeSelf))
                            {
                                EditorGUILayout.LabelField(new GUIContent(current.gameObject.name, current.Id));
                            }
                            if (current.gameObject.activeSelf && EditorGUILayoutUtilities.CreateClickZone(Event.current, GUILayoutUtility.GetLastRect(), MouseCursor.Link, 0))
                            {
                                Selection.activeGameObject = current.gameObject;
                                if (current is Hotspot hotspot)
                                {
                                    SceneView.lastActiveSceneView.LookAt(hotspot.transform.position + hotspot.CameraPositionOffset, hotspot.transform.rotation, 0.01f);
                                }
                                else
                                {
                                    // Always oppsite to the transform.forward
                                    Vector3 targetPosition = current.transform.position;
                                    Vector3 cameraPosition = current.transform.position + current.transform.forward * 1;
                                    SceneView.lastActiveSceneView.LookAt(cameraPosition, Quaternion.LookRotation(targetPosition - cameraPosition), 1);
                                }

                            }
                            //using (new EditorGUI.DisabledGroupScope(!current.gameObject.activeSelf))
                            //{
                            //    if (GUILayout.Button(Icons.menu, EditorStyles.label, GUILayout.Width(20), GUILayout.Height(20)))
                            //    {
                            //        ShowObjectMenu(current);
                            //    };
                            //}
                        }
                    }
                }
            }
        }

        void ShowObjectsMenu<T>(IList<T> objects) where T : TreasuredObject
        {
            GenericMenu menu = new GenericMenu();
            if (typeof(T) == typeof(Hotspot))
            {
                menu.AddItem(Styles.snapAllToGround, false, () =>
                {
                    foreach (var hotspot in objects as IList<Hotspot>)
                    {
                        hotspot.SnapToGround();
                    }
                });
                menu.AddSeparator("");
            }
            menu.AddItem(Styles.selectAll, false, () =>
            {
                Selection.objects = objects.Select(x => x.gameObject).ToArray();
            });
            menu.ShowAsContext();
        }

        private class CustomMenuItem
        {
            public GUIContent content;
            public bool on;
            public GenericMenu.MenuFunction func;
        }

        private bool IsAllRequiredFieldFilled()
        {
            bool allFilled = !string.IsNullOrEmpty(_title.stringValue.Trim());
            allFilled &= !string.IsNullOrEmpty(_description.stringValue.Trim());
            return allFilled;
        }

        #region Context Menu
        /// <summary>
        /// Get the child with the name. Create a new game object if child not found.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        static Transform GetChild(Transform parent, string name)
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

        [MenuItem("GameObject/Treasured/Create Map from Json", false, 49)]
        static void CreateMapFromJson()
        {
            string jsonPath = EditorUtility.OpenFilePanel("Select Json", Utility.ProjectPath, "json");
            if (!File.Exists(jsonPath))
            {
                return;
            }
            string json = File.ReadAllText(jsonPath);
            try
            {
                TreasuredMapData data = JsonConvert.DeserializeObject<TreasuredMapData>(json);
                GameObject mapGO = new GameObject("Treasured Map");
                TreasuredMap map = mapGO.AddComponent<TreasuredMap>();
                map.Populate(data);

                GameObject hotspotRoot = new GameObject("Hotspots");
                hotspotRoot.transform.SetParent(mapGO.transform);

                GameObject interactableRoot = new GameObject("Interactables");
                interactableRoot.transform.SetParent(mapGO.transform);

                for (int i = 0; i < data.Hotspots.Count; i++)
                {
                    CreateTreasuredObject<Hotspot>(hotspotRoot.transform).BindData(data.Hotspots[i]);
                }

                for (int i = 0; i < data.Interactables.Count; i++)
                {
                    CreateTreasuredObject<Interactable>(interactableRoot.transform).BindData(data.Interactables[i]);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.StackTrace);
                throw e;
            }
        }

        [MenuItem("GameObject/Treasured/Create Map from Json", true, 49)]
        static bool CanCreateMapFromJson()
        {
            return Selection.activeGameObject == null;
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
