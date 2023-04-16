using System;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Reflection;
using UnityEditorInternal;
using UnityEditor.EditorTools;
using UnityEditor.ShortcutManagement;
using System.Collections.Generic;

namespace Treasured.UnitySdk
{
    //[EditorTool("Treasured Scene Tool", typeof(TreasuredMap))]
    class TreasuredSceneTool : EditorTool, IDrawSelectedHandles
    {
        static class Styles
        {
            public static readonly GUIContent[] icons = new GUIContent[]
            {
                EditorGUIUtility.TrIconContent("d_CircleCollider2D Icon", "Hotspot"),
                EditorGUIUtility.TrIconContent("GameObject On Icon", "Interactable"),
                EditorGUIUtility.TrIconContent("d_SceneViewAudio", "Sound Source"),
                EditorGUIUtility.TrIconContent("d_Profiler.Video", "Video Renderer"),
                EditorGUIUtility.TrIconContent("d_BuildSettings.WebGL", "HTML Embed")
            };

            public static readonly GUIContent logo = new GUIContent(Resources.Load<Texture2D>("Treasured_Logo"));
        }

        static readonly Type[] ObjectTypes = new Type[] { typeof(Hotspot), typeof(Interactable), typeof(SoundSource), typeof(VideoRenderer), typeof(HTMLEmbed) };
        static string[] ObjectTypeNames;

        int selectedIndex;
        TreasuredMap scene;
        TreasuredObject selection;
        bool showPath;
        Rect windowRect = new Rect(10, 100, 100, 200);

        public override GUIContent toolbarIcon
        {
            get
            {
                return Styles.logo;
            }
        }

        // Global tools (tools that do not specify a target type in the attribute) are lazy initialized and persisted by
        // a ToolManager. Component tools (like this example) are instantiated and destroyed with the current selection.
        void OnEnable()
        {
            // Allocate unmanaged resources or perform one-time set up functions here
            ObjectTypeNames = ObjectTypeNames = ObjectTypes.Select(t => ObjectNames.NicifyVariableName(t.Name)).ToArray();
            scene = target as TreasuredMap;
        }

        void OnDisable()
        {
            // Free unmanaged resources, state teardown.
        }

        // The second "context" argument accepts an EditorWindow type.
        [Shortcut("Activate Treasured Scene Tool", typeof(SceneView), KeyCode.T, ShortcutModifiers.Action)]
        static void PlatformToolShortcut()
        {
            if (Selection.GetFiltered<TreasuredMap>(SelectionMode.TopLevel).Length > 0)
                ToolManager.SetActiveTool<TreasuredSceneTool>();
            else
                Debug.Log("No scene selected!");
        }

        // Called when the active tool is set to this tool instance. Global tools are persisted by the ToolManager,
        // so usually you would use OnEnable and OnDisable to manage native resources, and OnActivated/OnWillBeDeactivated
        // to set up state. See also `EditorTools.{ activeToolChanged, activeToolChanged }` events.
        public override void OnActivated()
        {
            SceneView.lastActiveSceneView.ShowNotification(new GUIContent("Entering Treasured Scene Tool"), .1f);
        }

        // Called before the active tool is changed, or destroyed. The exception to this rule is if you have manually
        // destroyed this tool (ex, calling `Destroy(this)` will skip the OnWillBeDeactivated invocation).
        public override void OnWillBeDeactivated()
        {
            SceneView.lastActiveSceneView.ShowNotification(new GUIContent("Exiting Treasured Scene Tool"), .1f);
        }

        // Equivalent to Editor.OnSceneGUI.
        public override void OnToolGUI(EditorWindow window)
        {
            if (!(window is SceneView sceneView))
                return;

            int id = GUIUtility.GetControlID(FocusType.Passive);
            HandleUtility.AddDefaultControl(id);
            Handles.BeginGUI();
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                EditorGUI.BeginChangeCheck();
                selectedIndex = GUILayout.SelectionGrid(selectedIndex, Styles.icons, Styles.icons.Length, EditorStyles.toolbarButton);
                if (EditorGUI.EndChangeCheck())
                {
                    SceneView.lastActiveSceneView.ShowNotification(new GUIContent($"Click on where you want to place the {ObjectTypeNames[selectedIndex]}"), 1.5f);
                }
                GUILayout.FlexibleSpace();
            }
            GUILayout.Window(1, windowRect, (id) =>
            {
                showPath = GUILayout.Toggle(showPath, "Show Hotspot Path");
            }, "Scene");

            Event e = Event.current;
            if (e.type == EventType.MouseDown && e.button == 0 && !windowRect.Contains(e.mousePosition))
            {
                selection = CreateObject(ObjectTypes[selectedIndex]);
            }
            Handles.EndGUI();

            if (selectedIndex == 0)
            {
                var hotspots = scene.GetComponentsInChildren<Hotspot>();
                for (int i = 0; i < hotspots.Length; i++)
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

                    if (!scene.Loop && i == hotspots.Length - 1)
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
        }

        private Hotspot GetNextActiveHotspot(int currentIndex, IList<Hotspot> list)
        {
            int index = currentIndex;
            Hotspot current = list[index];
            Hotspot next = list[(index + 1) % list.Count];
            while (next != current)
            {
                if (index == list.Count - 1 && !scene.Loop)
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

        // IDrawSelectedHandles interface allows tools to draw gizmos when the target objects are selected, but the tool
        // has not yet been activated. This allows you to keep MonoBehaviour free of debug and gizmo code.
        public void OnDrawHandles()
        {
            //foreach (var obj in targets)
            //{
            //    if (obj is Platform platform)
            //        Handles.DrawLine(platform.start, platform.end, 6f);
            //}
        }

        TreasuredObject CreateObject(Type t)
        {
            if (!typeof(TreasuredObject).IsAssignableFrom(t))
            {
                throw new ArgumentException($"Type dismatch. {t.Name} is not a type of TreasuredObject.");
            }
            TreasuredMap scene = (target as TreasuredMap);
            if (scene == null)
            {
                return null;
            }
            string categoryName = ObjectNames.NicifyVariableName(t.Name + "s");
            Transform categoryRoot = scene.transform.Find(categoryName);
            if (categoryRoot == null)
            {
                categoryRoot = new GameObject(categoryName).transform;
                categoryRoot.SetParent(scene.transform);
            }
            string uniqueName = UnityEditor.GameObjectUtility.GetUniqueNameForSibling(categoryRoot, ObjectNames.NicifyVariableName(t.Name));
            GameObject newGO = new GameObject(uniqueName);
            Undo.RegisterCreatedObjectUndo(newGO, $"Create {uniqueName}");
            TreasuredObject obj = (TreasuredObject)newGO.AddComponent(t);
            newGO.transform.SetParent(categoryRoot);
            obj.TryInvokeMethods("OnSelectedInHierarchy");
            // Place the new game object on floor if collider found.
            Camera camera = SceneView.lastActiveSceneView.camera;
            if (Physics.Raycast(HandleUtility.GUIPointToWorldRay(Event.current.mousePosition), out var hit))
            {
                obj.transform.position = hit.point;
                if (obj is Hotspot hotspot)
                {
                    hotspot.Camera.transform.position = hit.point + new Vector3(0, 1.5f, 0);
                    hotspot.Camera.transform.localRotation = Quaternion.identity;
                }
                else if (obj is VideoRenderer videoRenderer)
                {
                    videoRenderer.Hitbox.transform.localScale = new Vector3(1, 1, 0.01f);
                }
            }
            else
            {
                SceneView.lastActiveSceneView.ShowNotification(new GUIContent("No collider found, placed at origin (0, 0, 0)"), 2f);
            }
            return obj;
        }
    }
        public class TreasuredSceneBuilderWindow : EditorWindow
    {
        public static class Styles
        {
            public struct GUIContentWithType
            {
                public Type type;
                public GUIContent guiContent;
            }

            public static readonly GUIContent snapToGround = EditorGUIUtility.TrTextContent("Snap on ground", "Snap the object slightly above the ground from camera position. This also snap the first box collider to the ground based on the size.");
            public static readonly GUIContent recordingText = new GUIContent("Click on the scene to change the default view for Hotspot");
            public static readonly GUIStyle pathLabel = new GUIStyle(EditorStyles.linkLabel)
            {
                normal = new GUIStyleState()
                {
                    textColor = EditorStyles.label.normal.textColor,
                }
            };
            public static readonly GUIStyle radioButton = new GUIStyle(EditorStyles.radioButton)
            {
                padding = new RectOffset(20, 0, 0, 0)
            };

            public static readonly GUIStyle centeredGreyMiniLabel = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
            {
                wordWrap = true
            };

            public static readonly GUIContent[] icons = new GUIContent[]
                {
                EditorGUIUtility.TrIconContent(Resources.Load<Texture2D>("Hotspot"), "Hotspot"),
                EditorGUIUtility.TrIconContent("GameObject On Icon", "Interactable"),
                EditorGUIUtility.TrIconContent("d_SceneViewAudio", "Sound Source"),
                EditorGUIUtility.TrIconContent("d_Profiler.Video", "Video Renderer"),
                EditorGUIUtility.TrIconContent("d_BuildSettings.WebGL", "HTML Embed")
                };

            public static readonly GUIContent logo = new GUIContent(Resources.Load<Texture2D>("Treasured_Logo"));
        }

        private static readonly Type[] ObjectTypes = new Type[] { typeof(Hotspot), typeof(Interactable), typeof(SoundSource), typeof(VideoRenderer), typeof(HTMLEmbed) };
        //private static string[] ObjectTypeNames;


        [MenuItem("Tools/Treasured/Scene Builder", priority = 0)]
        static TreasuredSceneBuilderWindow ShowWindow()
        {
            var window = EditorWindow.GetWindow<TreasuredSceneBuilderWindow>();
            window.titleContent = new GUIContent("Treasured Scene Builder", Resources.Load<Texture2D>("Treasured_Logo"));
            window.Show();
            return window;
        }

        public static void ShowWindow(TreasuredMap scene)
        {
            var window = ShowWindow();
            window.scene = scene;
            window.selection = scene;
        }

        struct GizmosState
        {
            public bool enabled;
            public Color color;
        }

        class HotspotGizmosState
        {
            public GizmosState camera = new GizmosState() { color = Color.red };
            public GizmosState path = new GizmosState() { color = Color.white };
            public GizmosState hitbox = new GizmosState() { color = Color.green };
        }

        public TreasuredMap scene;

        ToolMode toolMode;
        int selectedTypeIndex;
        Component selection;

        HotspotGizmosState hotspotGizmosState = new HotspotGizmosState();

        ReorderableList objectList;

        struct ListItem
        {
            public bool Enabled
            {
                get
                {
                    return target.gameObject.activeSelf;
                }
                set
                {
                    target.gameObject.SetActive(value);
                }
            }
            public TreasuredObject target;
        }

        public enum ToolMode
        {
            None,
            Create,
            Record
        }

        private void OnEnable()
        {
            if (!Selection.activeGameObject.IsNullOrNone() && (Selection.activeGameObject.TryGetComponent<TreasuredMap>(out var map) || !(map = Selection.activeGameObject.GetComponentInParent<TreasuredMap>()).IsNullOrNone()))
            {
                scene = map;
            }
            if (scene.IsNullOrNone())
            {
                scene = GameObject.FindObjectOfType<TreasuredMap>();
            }
            ShowSceneViewNotification(new GUIContent("Click on where you want to place the object."), 1.5f);
            SceneView.duringSceneGui -= OnSceneViewGUI;
            SceneView.duringSceneGui += OnSceneViewGUI;

            //if (scene.IsNullOrNone() && Selection.activeGameObject.TryGetComponent<TreasuredMap>(out var component))
            //{
            //    scene = component;
            //}
            //selection = scene;

            //objectList = new ReorderableList(scene.GetComponentsInChildren<Hotspot>().Select(x => new ListItem()
            //{
            //    target = x
            //}).ToList(), typeof(ListItem));
            //objectList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            //{
            //    ListItem item = (ListItem)objectList.list[index];
            //    item.Enabled = EditorGUI.ToggleLeft(rect, item.target.name, item.Enabled);
            //};
            toolMode = ToolMode.Create;
            OnSelectionChange();
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneViewGUI;
        }

        private void OnSceneViewGUI(SceneView sceneView)
        {
            // Prevent from selecting object in Scene View
            int id = GUIUtility.GetControlID(FocusType.Passive);
            HandleUtility.AddDefaultControl(id);
            Handles.BeginGUI();
            ProcessEvents();
            switch (selection)
            {
                case Hotspot hotspot:
                    //Handles.DrawCamera(new Rect(Screen.width - 110, Screen.height - 130, 100, 100), Camera.main);
                    break;
                default:
                    break;
            }
            //if (selection is TreasuredMap map)
            //{
            //    if (hotspotGizmosState.camera.enabled)
            //    {
            //        //Color tempColor = Gizmos.color;
            //        //Matrix4x4 tempMatrix = Gizmos.matrix;
            //        //Gizmos.color = hotspotGizmosState.camera.color;
            //        //foreach (var hotspot in map.Hotspots)
            //        //{
            //        //    Debug.LogError("a");
            //        //     = Matrix4x4.TRS(hotspot.transform.position, hotspot.transform.rotation, Vector3.one);
            //        //    Gizmos.DrawFrustum(Vector3.zero, 25, 0, 0.5f, 3);
            //        //    Gizmos.color = tempColor;
            //        //}
            //        //Gizmos.matrix = tempMatrix;
            //    }
            //}
            Handles.EndGUI();
        }

        private void OnSelectionChange()
        {
            if (Selection.objects.Length != 1)
            {
                return;
            }
            selection = null;
            foreach (var type in ObjectTypes)
            {
                if(Selection.activeGameObject.TryGetComponent(type, out var component))
                {
                    this.selection = component;
                    break;
                }
            }

            //switch (selection)
            //{
            //    case TreasuredMap map:
            //        toolMode = ToolMode.Create;
            //        break;
            //    default:
            //        toolMode = ToolMode.None;
            //        break;
            //}

            this.Repaint();
        }

        private void OnGUI()
        {
            if (GUILayout.Button("Create New Scene", GUILayout.Height(24)))
            {
                scene = CreateNewTreasuredScene();
            }
            scene = (TreasuredMap)EditorGUILayout.ObjectField(new GUIContent("Scene"), scene, typeof(TreasuredMap), true);
            if (scene == null)
            {
                EditorGUILayout.LabelField("You have not selected a scene yet. Please choose one from the hierarchy or create a new scene.", Styles.centeredGreyMiniLabel, GUILayout.ExpandHeight(true));
                return;
            }
            if (GUILayout.Button("Select All Hitbox"))
            {
                Selection.objects = scene.Hotspots.Select(hs => hs.Hitbox.gameObject).ToArray();
            }
            if (GUILayout.Button("Select All Camera"))
            {
                Selection.objects = scene.Hotspots.Select(hs => hs.Camera.gameObject).ToArray();
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel("Object Type", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                EditorGUI.BeginChangeCheck();
                selectedTypeIndex = GUILayout.SelectionGrid(selectedTypeIndex, Styles.icons, Styles.icons.Length, EditorStyles.toolbarButton, GUILayout.Width(150f));
                if (EditorGUI.EndChangeCheck())
                {
                    ShowSceneViewNotification(new GUIContent($"Click on where you want to place the {Styles.icons[selectedTypeIndex].tooltip}."), 1.5f);
                }
                EditorGUI.indentLevel--;
            }
            if (GUILayout.Button("Select All"))
            {
                Selection.objects = scene.GetComponentsInChildren(ObjectTypes[selectedTypeIndex]).Select(c => c.gameObject).ToArray();
            }
            switch (selection)
            {
                case TreasuredMap scene:
                    
                    break;
                case Hotspot hotspot:
                    EditorGUILayout.LabelField("Hitbox Controls", EditorStyles.boldLabel);
                    foreach (var method in ReflectionUtilities.GetMethodsWithAttribute<ControlAttribute>(hotspot))
                    {
                        if (GUILayout.Button(method.attribute.name))
                        {
                            method.methodInfo.Invoke(hotspot, new object[] { });
                        }
                    }
                    if (GUILayout.Button("Snap to Ground"))
                    {
                        hotspot.SnapToGround();
                    }
                    EditorGUILayout.LabelField("Camera Controls", EditorStyles.boldLabel);
                    hotspot.Camera.transform.position = EditorGUILayout.Vector3Field("Position", hotspot.Camera.transform.position);
                    if (GUILayout.Button(toolMode != ToolMode.Record ? "Record" : "Stop Record"))
                    {
                        if(toolMode == ToolMode.Record)
                        {
                            toolMode = ToolMode.None;
                        }
                        else
                        {
                            ShowSceneViewNotification(Styles.recordingText, 5);
                            toolMode = ToolMode.Record;
                            PreviewCamera(hotspot.Camera);
                        }
                    }
                    if (GUILayout.Button("Preview Camera"))
                    {
                        PreviewCamera(hotspot.Camera);
                    }
                    EditorGUI.indentLevel--;
                    break;
                case Interactable interactable:
                    break;
            }
            //objectList.DoLayoutList();
        }

        void ShowSceneViewNotification(GUIContent notification, double fadeoutWait)
        {
            SceneView.lastActiveSceneView?.ShowNotification(notification, fadeoutWait);
            SceneView.lastActiveSceneView?.Repaint();
        }

        TreasuredObject TryCreateObject(Type t, Vector3 position)
        {
            if (!typeof(TreasuredObject).IsAssignableFrom(t))
            {
                throw new ArgumentException($"Type dismatch. {t.Name} is not a type of TreasuredObject.");
            }
            string categoryName = ObjectNames.NicifyVariableName(t.Name + "s");
            Transform categoryRoot = scene.transform.Find(categoryName);
            if (categoryRoot == null)
            {
                categoryRoot = new GameObject(categoryName).transform;
                categoryRoot.SetParent(scene.transform);
            }
            string uniqueName = UnityEditor.GameObjectUtility.GetUniqueNameForSibling(categoryRoot, ObjectNames.NicifyVariableName(t.Name));
            GameObject newGO = new GameObject(uniqueName);
            Undo.RegisterCreatedObjectUndo(newGO, $"Create {uniqueName}");
            TreasuredObject obj = (TreasuredObject)newGO.AddComponent(t);
            newGO.transform.SetParent(categoryRoot);
            obj.TryInvokeMethods("OnSelectedInHierarchy");
            EditorGUIUtility.PingObject(obj);
            obj.transform.position = position;
            if (obj is Hotspot hotspot)
            {
                if (hotspot.Hitbox.TryGetComponent<Collider>(out var collider))
                {
                    obj.Hitbox.transform.position = new Vector3(obj.Hitbox.transform.position.x, obj.Hitbox.transform.position.y + collider.bounds.size.y / 2, obj.Hitbox.transform.position.z);
                }
                hotspot.Camera.transform.position = position + new Vector3(0, 1.5f, 0);
                hotspot.Camera.transform.localRotation = Quaternion.identity;
            }
            else if (obj is VideoRenderer videoRenderer)
            {
                videoRenderer.Hitbox.transform.localScale = new Vector3(1, 1, 0.01f);
            }
            return obj;
        }

        void ProcessEvents()
        {
            Event e = Event.current;
            switch (toolMode)
            {
                case ToolMode.Create:
                    if (e.type == EventType.MouseDown)
                    {
                        if (selectedTypeIndex > -1 && selectedTypeIndex < ObjectTypes.Length)
                        {
                            if (e.button == 0)
                            {

                                // Place the new game object on floor if collider found.
                                if (Physics.Raycast(HandleUtility.GUIPointToWorldRay(Event.current.mousePosition), out var hit))
                                {
                                    var to = hit.transform.GetComponentInParent<TreasuredObject>();
                                    if (!to)
                                    {
                                        to = TryCreateObject(ObjectTypes[selectedTypeIndex], hit.point);
                                        SceneView.lastActiveSceneView?.Repaint();
                                    }
                                    Selection.activeObject = to;
                                }
                                else
                                {
                                    SceneView.lastActiveSceneView.ShowNotification(new GUIContent($"Invalid Placement"), .1f);
                                }
                            }
                            //else if(e.button == 1)
                            //{
                            //    if (Physics.Raycast(HandleUtility.GUIPointToWorldRay(e.mousePosition), out var hit))
                            //    {
                            //        if (hit.transform.TryGetComponent<Hitbox>(out var obj))
                            //        {
                            //            GenericMenu menu = new GenericMenu();
                            //            menu.AddItem(new GUIContent("Remove"), false, () =>
                            //            {
                            //                GameObject.DestroyImmediate(obj.GetComponentInParent<TreasuredObject>().gameObject);
                            //            });
                            //            menu.ShowAsContext();
                            //        }
                            //    }
                                
                            //}
                        }
                    }
                    break;
                case ToolMode.Record:
                    switch (e.type)
                    {
                        case EventType.MouseDown:
                            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                            if (e.button == 0 && selection is Hotspot hotspot)
                            {
                                hotspot.Camera.transform.rotation = Quaternion.LookRotation(ray.direction);
                                PreviewCamera(hotspot.Camera);
                            }
                            break;
                        case EventType.Layout:
                            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                            break;
                    }
                    break;
            }
        }

        TreasuredMap CreateNewTreasuredScene()
        {
            GameObject root = new GameObject("Treasured Scene");
            TreasuredMap scene = root.AddComponent<TreasuredMap>();
            return scene;
        }

        void PreviewCamera(HotspotCamera camera, float size = 0.01f)
        {
            SceneView.lastActiveSceneView.LookAt(camera.transform.position, camera.transform.rotation, size);
        }
    }
}
