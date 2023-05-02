using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    public class SceneEditor : ScriptableObject
    {
        static class Styles
        {
            public static readonly GUIContent[] icons = new GUIContent[]
            {
                EditorGUIUtility.TrIconContent(Resources.Load<Texture2D>("Hotspot"), "Hotspot"),
                EditorGUIUtility.TrIconContent("GameObject On Icon", "Interactable"),
                EditorGUIUtility.TrIconContent("d_SceneViewAudio", "Sound Source"),
                EditorGUIUtility.TrIconContent("d_Profiler.Video", "Video Renderer"),
                EditorGUIUtility.TrIconContent("d_BuildSettings.WebGL", "HTML Embed")
            };

            public static readonly GUIContent[] mode = new GUIContent[]
            {
                EditorGUIUtility.TrIconContent("CreateAddNew", "Create New"),
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
        }

        public enum EditorMode
        {
            CreateNew,
            EditTransform
        }

        public TreasuredMap map;
        int selectedTypeIndex = 0;
        EditorMode editorMode;
        TreasuredObject editingTarget;
        ObjectListState[] objectListStates;

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
            public List<TreasuredObject> objects;
            public Vector2 scrollPosition;
            public GroupToggleState toggleState;
            public bool enableAll;

            public void UpdateObjectList(TreasuredMap map)
            {
                if (this.objects == null)
                {
                    this.objects = new List<TreasuredObject>();
                }
                var objects = map.GetComponentsInChildren(type, true);
                foreach (var obj in objects)
                {
                    if (!this.objects.Contains(obj))
                    {
                        this.objects.Add((TreasuredObject)obj);
                    }
                }
            }
            
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui -= OnSceneView;
            SceneView.duringSceneGui += OnSceneView;
        }

        public void SetMap(TreasuredMap map)
        {
            this.map = map;
            InitializeObjectListStates();
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneView;
        }

        void InitializeObjectListStates()
        {
            objectListStates = new ObjectListState[ObjectTypes.Length];
            for (int i = 0; i < ObjectTypes.Length; i++)
            {
                objectListStates[i] = new ObjectListState()
                {
                    type = ObjectTypes[i]
                };
                objectListStates[i].UpdateObjectList(map);
            }
        }

        void OnSceneView(SceneView sceneView)
        {
            // Prevent from selecting object in Scene View
            int id = GUIUtility.GetControlID(FocusType.Passive);
            HandleUtility.AddDefaultControl(id);
            Handles.BeginGUI();
            ProcessEvents();
            Handles.EndGUI();
            if (editorMode == EditorMode.EditTransform && editingTarget != null)
            {
                // TODO: Add Hotspot Camera Preview using Overlays on version 2021.3
                switch (Tools.current)
                {
                    case Tool.Move:
                        Transform transform = editingTarget.transform;
                        EditorGUI.BeginChangeCheck();
                        Vector3 newPosition = Handles.PositionHandle(transform.position, transform.rotation);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(transform, $"Move {editingTarget.name}");
                            transform.position = newPosition;
                        }
                        switch (editingTarget)
                        {
                            case Hotspot hotspot:
                                Transform cameraTransform = hotspot.Camera.transform;
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
                        switch (editingTarget)
                        {
                            case Hotspot hotspot:
                                //Screen.width - 220, Screen.height - 250, 200, 150
                                Transform cameraTransform = hotspot.Camera.transform;
                                EditorGUI.BeginChangeCheck();
                                Quaternion newCameraRotation = Handles.RotationHandle(cameraTransform.rotation, cameraTransform.position);
                                if (EditorGUI.EndChangeCheck())
                                {
                                    Undo.RecordObject(cameraTransform, "Rotate Hotspot Camera");
                                    cameraTransform.rotation = newCameraRotation;
                                }
                                float size = HandleUtility.GetHandleSize(hotspot.transform.position);
                                Handles.color = Color.blue;
                                Handles.ArrowHandleCap(0, cameraTransform.position, cameraTransform.rotation, size, EventType.Repaint);
                                break;
                        }
                        break;
                    case Tool.Scale:
                        break;
                }
            }
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
                                        TryCreateObject(ObjectTypes[selectedTypeIndex], hit.point);
                                    }
                                }
                                else
                                {
                                    // Place object at origin if no hit found.
                                    TryCreateObject(ObjectTypes[selectedTypeIndex], Vector3.zero);
                                }
                                objectListStates[selectedTypeIndex].UpdateObjectList(map);
                                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                            }
                        }
                    }
                    break;
                case EditorMode.EditTransform:
                    break;
            }
        }

        public void OnGUI()
        {
            EditorGUILayout.PrefixLabel("Object Type", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            selectedTypeIndex = GUILayout.SelectionGrid(selectedTypeIndex, Styles.icons, Styles.icons.Length, GUILayout.Height(32f));
            if (EditorGUI.EndChangeCheck())
            {
                switch (editorMode)
                {
                    case EditorMode.CreateNew:
                        SceneView.lastActiveSceneView.ShowNotification(new GUIContent($"Click on where you want to place the {Styles.icons[selectedTypeIndex].tooltip}"), 1.5f);
                        break;
                    case EditorMode.EditTransform:
                        SceneView.lastActiveSceneView.ShowNotification(new GUIContent($"Click on the object you want to edit from the list below"), 1.5f);
                        break;
                }
            }
            EditorGUILayout.PrefixLabel("Mode", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            var state = objectListStates[selectedTypeIndex];
            editorMode = (EditorMode)GUILayout.SelectionGrid((int)editorMode, Styles.mode, Styles.icons.Length, GUILayout.Height(32f));
            if (EditorGUI.EndChangeCheck())
            {
                if (editorMode == EditorMode.EditTransform && editingTarget == null && objectListStates[selectedTypeIndex].objects.Count > 0)
                {
                    editingTarget = objectListStates[selectedTypeIndex].objects[0];
                }
            }
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
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    int activeCount = state.objects.Count(x => x.gameObject.activeSelf);
                    if (activeCount == state.objects.Count)
                    {
                        state.toggleState = GroupToggleState.All;
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
                           GUILayout.Height(state.objects.Count == 0 ? 20: Mathf.Clamp(state.objects.Count * 20, state.objects.Count * 20, 300))))
                {
                    state.scrollPosition = scope.scrollPosition;
                    for (int index = 0; index < state.objects.Count; index++)
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            TreasuredObject current = state.objects[index];
                            using(new EditorGUILayout.VerticalScope())
                            {
                                using(new EditorGUILayout.HorizontalScope())
                                {
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
                                                style: editingTarget == current ? Styles.selectedObjectLabel : Styles.objectLabel);
                                        }
                                    }
                                }
                                if (editingTarget == current)
                                {
                                    using (new GUILayout.HorizontalScope())
                                    {
                                        GUILayout.FlexibleSpace();
                                        if (current is Hotspot hotspot)
                                        {
                                            if (GUILayout.Button("Preview"))
                                            {
                                                SceneView.lastActiveSceneView.LookAt(hotspot.Camera.transform.position, hotspot.Camera.transform.rotation, 0.01f);
                                            }
                                            if (GUILayout.Button("Snap Hitbox"))
                                            {
                                                hotspot.SnapToGround();
                                            }
                                        }
                                        
                                        GUILayout.FlexibleSpace();
                                    }
                                }
                            }

                            switch (EditorGUILayoutUtils.CreateClickZone(Event.current,
                                        GUILayoutUtility.GetLastRect(), MouseCursor.Link))
                            {
                                case 0:
                                    EditorGUIUtility.PingObject(current);
                                    editorMode = EditorMode.EditTransform;
                                    editingTarget = current;
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

        TreasuredObject TryCreateObject(Type t, Vector3 position)
        {
            if (!typeof(TreasuredObject).IsAssignableFrom(t))
            {
                throw new ArgumentException($"Type dismatch. {t.Name} is not a type of TreasuredObject.");
            }
            string categoryName = ObjectNames.NicifyVariableName(t.Name + "s");
            Transform categoryRoot = map.transform.Find(categoryName);
            if (categoryRoot == null)
            {
                categoryRoot = new GameObject(categoryName).transform;
                categoryRoot.SetParent(map.transform);
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
    }
}
