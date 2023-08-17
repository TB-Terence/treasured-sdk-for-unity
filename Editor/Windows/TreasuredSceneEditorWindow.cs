using System;
using System.Collections.Generic;
using System.Linq;
using Treasured.UnitySdk.Utilities;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    class TreasuredSceneEditorWindow : EditorWindow
    {
        static class Styles
        {
            public static readonly GUIContent[] icons = new GUIContent[]
            {
                EditorGUIUtility.TrIconContent(Resources.Load<Texture2D>("Hotspot"), "[Hotspot] Hotspots are used to allow the user to navigate through the scene."),
                EditorGUIUtility.TrIconContent("GameObject On Icon", "[Interactable] Interactables are used to allow the user to click to interact with the scene in the browser."),
                EditorGUIUtility.TrIconContent("d_SceneViewAudio", "Sound Source"),
                EditorGUIUtility.TrIconContent("d_Profiler.Video", "Video Renderer"),
                EditorGUIUtility.TrIconContent("d_BuildSettings.WebGL", "HTML Embed")
            };

            public static readonly GUIContent plus = EditorGUIUtility.TrIconContent("CreateAddNew", "Create New");
            public static readonly GUIContent snapHitboxButton = EditorGUIUtility.TrIconContent("d_RaycastCollider Icon", "Snap Hitbox");

            public static readonly GUIContent[] mode = new GUIContent[]
            {
                plus,
                EditorGUIUtility.TrIconContent("d_Transform Icon", "Edit"),
            };

            public static readonly GUIContent logo = new GUIContent(Resources.Load<Texture2D>("Treasured_Logo"));

            public static readonly GUIStyle objectLabel = new GUIStyle("label")
            {
                fontStyle = FontStyle.Bold
            };

            public static readonly GUIStyle selectedObjectLabel = new GUIStyle(objectLabel)
            {
                fontStyle = FontStyle.Bold,
                normal = new GUIStyleState()
                {
                    background = Texture2D.grayTexture
                }
            };

            public static readonly GUIStyle wordWrapCenteredGreyMiniLabel = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
            {
                wordWrap = true
            };

            public const string kClickToPreview = "Click to Preview";
        }

        [MenuItem("Tools/Treasured/Scene Editor", priority = 0)]
        static TreasuredSceneEditorWindow ShowWindow()
        {
            if (Selection.activeGameObject == null)
            {
                return ShowWindow(null);
            }
            var scene = Selection.activeGameObject.GetComponent<TreasuredScene>();
            if (scene == null)
            {
                scene = Selection.activeGameObject.GetComponentInParent<TreasuredScene>();
            }
            return ShowWindow(scene);
        }

        public static TreasuredSceneEditorWindow ShowWindow(TreasuredScene scene)
        {
            var window = EditorWindow.GetWindow<TreasuredSceneEditorWindow>();
            window.scene = scene;
            window.titleContent = new GUIContent("Treasured Scene Editor", Resources.Load<Texture2D>("Treasured_Logo"));
            window.InitializeObjectListStates();
            window.Show();
            window.minSize = new Vector2(230, 120);
            window.maxSize = new Vector2(230, 120);
            return window;
        }

        public enum EditorMode
        {
            CreateNew,
            Edit,
            Record
        }

        public TreasuredScene scene;
        int selectedTypeIndex = 0;
        int selectedModeIndex = 0;
        EditorMode editorMode;
        /// <summary>
        /// Getter for EditingTarget, use EditingTarget to set value to automatically change Selection.activeGameObject and EditorMode
        /// </summary>
        TreasuredObject _editingTarget;
        public TreasuredObject EditingTarget
        {
            get
            {
                return _editingTarget;
            }
            set
            {
                _editingTarget = value;
                if(_editingTarget != null)
                {
                    Selection.activeGameObject = _editingTarget.gameObject;
                    //editorMode = EditorMode.Edit;
                }
            }
        }
        ObjectListState[] objectListStates;
        int zooming = 1;
        bool showHotspotPath = false;

        bool enableCameraPreview;

        bool enableClickToCreate = false;

        private static readonly Type[] ObjectTypes = new Type[] { typeof(Hotspot), typeof(Interactable), typeof(SoundSource), typeof(VideoRenderer), typeof(HTMLEmbed) };

        public enum GroupToggleState
        {
            None,
            All,
            Mixed
        }

        public class ObjectListState
        {
            public Type type;
            public TreasuredScene scene;
            public List<TreasuredObject> objects;
            public Vector2 scrollPosition;
            public GroupToggleState toggleState;
            public bool enableAll;

            public void UpdateObjectList()
            {
                if (this.objects == null)
                {
                    this.objects = new List<TreasuredObject>();
                }
                this.objects.Clear();
                var objects = scene.GetComponentsInChildren(type, true);
                foreach (var obj in objects)
                {
                    this.objects.Add((TreasuredObject)obj);
                }
            }

        }

        private void OnEnable()
        {
            InitializeObjectListStates();
            SceneView.duringSceneGui -= OnSceneView;
            SceneView.duringSceneGui += OnSceneView;
            Undo.undoRedoPerformed += () => { 
                objectListStates[selectedTypeIndex]?.UpdateObjectList(); 
            };
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneView;
        }

        void OnPreviewCamera()
        {
            if (Selection.activeGameObject is GameObject go && go.scene != null && go.TryGetComponent(out Hotspot hotspot))
            {
                EditorUtils.PreviewCamera(hotspot.Camera);
            }
        }

        void InitializeObjectListStates()
        {
            if (scene == null) 
            {
                return;
            }
            objectListStates = new ObjectListState[ObjectTypes.Length];
            for (int i = 0; i < ObjectTypes.Length; i++)
            {
                objectListStates[i] = new ObjectListState()
                {
                    type = ObjectTypes[i],
                    scene = scene
                };
                objectListStates[i].UpdateObjectList();
            }
        }

        void OnSceneView(SceneView sceneView)
        {
            if (scene == null)
            {
                return;
            }
            // Prevent from selecting object in Scene View
            int id = GUIUtility.GetControlID(FocusType.Passive);
            HandleUtility.AddDefaultControl(id);
            Handles.BeginGUI();
            ProcessEvents();
            Handles.EndGUI();
            switch (editorMode)
            {
                case EditorMode.CreateNew:
                    break;
                case EditorMode.Edit:
                    if (EditingTarget != null)
                    {
                        // TODO: Add Hotspot Camera Preview using Overlays on version 2021.3
                        switch (Tools.current)
                        {
                            case Tool.Move:
                                Transform transform = EditingTarget.transform;
                                EditorGUI.BeginChangeCheck();
                                Vector3 newPosition = Handles.PositionHandle(transform.position, transform.rotation);
                                if (EditorGUI.EndChangeCheck())
                                {
                                    Undo.RecordObject(transform, $"Move {EditingTarget.name}");
                                    transform.position = newPosition;
                                }
                                switch (EditingTarget)
                                {
                                    case Hotspot hs:
                                        Transform cameraTransform = hs.Camera.transform;
                                        EditorGUI.BeginChangeCheck();
                                        Vector3 newCameraPosition = Handles.PositionHandle(cameraTransform.position, cameraTransform.rotation);
                                        if (EditorGUI.EndChangeCheck())
                                        {
                                            Undo.RecordObject(cameraTransform, "Move Hotspot Camera");
                                            cameraTransform.position = newCameraPosition;
                                        }
                                        break;
                                }
                                break;
                            case Tool.Rotate:
                                switch (EditingTarget)
                                {
                                    case Hotspot hs:
                                        //Screen.width - 220, Screen.height - 250, 200, 150
                                        Transform cameraTransform = hs.Camera.transform;
                                        EditorGUI.BeginChangeCheck();
                                        Quaternion newCameraRotation = Handles.RotationHandle(cameraTransform.rotation, cameraTransform.position);
                                        if (EditorGUI.EndChangeCheck())
                                        {
                                            Undo.RecordObject(cameraTransform, "Rotate Hotspot Camera");
                                            cameraTransform.rotation = newCameraRotation;
                                        }
                                        float size = HandleUtility.GetHandleSize(hs.transform.position);
                                        Handles.color = Color.blue;
                                        Handles.ArrowHandleCap(0, cameraTransform.position, cameraTransform.rotation, size, EventType.Repaint);
                                        break;
                                }
                                break;
                            case Tool.Scale:
                                break;
                        }
                    }
                    break;
                case EditorMode.Record:
                    if (EditingTarget != null && EditingTarget is Hotspot hotspot)
                    {
                        if (Event.current.button == 0)
                        {
                            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                            hotspot.Camera.transform.rotation = Quaternion.LookRotation(ray.direction);
                            SceneView.lastActiveSceneView.LookAt(hotspot.Camera.transform.position, hotspot.Camera.transform.rotation, 0.01f);
                        }
                    }
                    break;
            }
            
            if(showHotspotPath) DrawHotspotPath();
        }

        void ProcessEvents()
        {
            Event e = Event.current;
            switch (editorMode)
            {
                case EditorMode.CreateNew:
                    if (e.type == EventType.MouseDown)
                    {
                        if (selectedTypeIndex > -1 && selectedTypeIndex < ObjectTypes.Length)
                        {
                            if (e.button == 0)
                            {
                                // Place the new game object on floor if collider found.
                                bool hasHit = Physics.Raycast(HandleUtility.GUIPointToWorldRay(Event.current.mousePosition), out var hit);
                                if (hasHit)
                                {
                                    // Check if we are clicking on existing TreasuredObject
                                    var to = hit.transform.GetComponentInParent<TreasuredObject>();
                                    if (!to)
                                    {
                                        EditingTarget = TryCreateObject(ObjectTypes[selectedTypeIndex], hit.point);
                                    }
                                }
                                else
                                {
                                    // Place object at origin if no hit found.
                                    EditingTarget = TryCreateObject(ObjectTypes[selectedTypeIndex], Vector3.zero);
                                }
                                objectListStates[selectedTypeIndex].UpdateObjectList();
                                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                            }
                        }
                    }
                    break;
                case EditorMode.Edit:
                    break;
            }
        }

        public void OnGUI()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                EditorGUI.BeginChangeCheck();
                float labelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 50;
                scene = (TreasuredScene)EditorGUILayout.ObjectField(new GUIContent("Scene"), scene, typeof(TreasuredScene), true);
                EditorGUIUtility.labelWidth = labelWidth;
                if (EditorGUI.EndChangeCheck())
                {
                    InitializeObjectListStates();
                }
                if (scene == null)
                {
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField("No scene is selected. Pick one from the hierarchy OR", Styles.wordWrapCenteredGreyMiniLabel, GUILayout.ExpandHeight(true));
                    if (GUILayout.Button("Create New Scene"))
                    {
                        GameObject newScene = new GameObject("Treasured Scene");
                        scene = newScene.AddComponent<TreasuredScene>();
                        Selection.activeGameObject = newScene;
                        InitializeObjectListStates();
                    }
                    GUILayout.FlexibleSpace();
                    return;
                }
                if (GUILayout.Button(new GUIContent("Overall View", "Zoom out the scene to give overall view.")))
                {
                    EditorUtils.Focus(10, scene.Hotspots.Select(x => x.transform).ToArray());
                }
                EditorGUI.BeginChangeCheck();
                enableCameraPreview = GUILayout.Toggle(enableCameraPreview, new GUIContent("Enable Camera Preview", "Move to Camera preview upon select Hotspot"));
                if (EditorGUI.EndChangeCheck())
                {
                    if (enableCameraPreview)
                    {
                        Selection.selectionChanged += OnPreviewCamera;
                        OnPreviewCamera();
                    }
                    else
                    {
                        Selection.selectionChanged -= OnPreviewCamera;
                    }
                }
                enableClickToCreate = GUILayout.Toggle(enableClickToCreate, new GUIContent("Enable Click to Create", "Click to Create allows you to quickly create selected type by clicking in the scene"));
                using (new EditorGUI.DisabledGroupScope(!enableClickToCreate))
                {
                    selectedTypeIndex = GUILayout.SelectionGrid(selectedTypeIndex, Styles.icons, Styles.icons.Length, GUILayout.Height(32));
                }
            }
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
            objectListStates[selectedTypeIndex].UpdateObjectList();
            return obj;
        }

        private void DrawHotspotPath()
        {
            if (selectedTypeIndex == 0)
            {
                var hotspots = scene.Hotspots;
                for (int i = 0; i < hotspots.Length; i++)
                {
                    Hotspot current = hotspots[i];
                    if (!current.gameObject.activeSelf)
                    {
                        continue;
                    }

                    Hotspot next = GetNextActiveHotspot(i, scene.Hotspots);

                    Transform hitboxTransform = current.Hitbox.transform;
                    Transform cameraTransform = current.Camera.transform;

                    if (Selection.activeGameObject != current.gameObject)
                    {
                        Handles.color = Color.white;
                        Handles.DrawDottedLine(hitboxTransform.position, cameraTransform.position, 5);
                    }

                    if (!scene.sceneInfo.loopHotspots && i == scene.Hotspots.Length - 1)
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
                if (index == list.Count - 1 && !scene.sceneInfo.loopHotspots)
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
