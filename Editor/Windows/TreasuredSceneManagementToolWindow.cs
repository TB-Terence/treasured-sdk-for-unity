using System;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Treasured.UnitySdk
{
    public class TreasuredSceneManagementToolWindow : EditorWindow
    {
        public static class Styles
        {
            public static readonly GUIContent snapToGround = EditorGUIUtility.TrTextContent("Snap on ground", "Snap the object slightly above the ground from camera position. This also snap the first box collider to the ground based on the size.");
            public static readonly GUIStyle pathLabel = new GUIStyle(EditorStyles.linkLabel)
            {
                normal = new GUIStyleState()
                {
                    textColor = EditorStyles.label.normal.textColor,
                }
            };
        }
        private static readonly Type[] ObjectTypes = new Type[] { typeof(Hotspot), typeof(Interactable), typeof(SoundSource), typeof(VideoRenderer) };
        private static string[] ObjectTypeNames;

        public static void ShowWindow(TreasuredMap scene)
        {
            var window = EditorWindow.GetWindow<TreasuredSceneManagementToolWindow>();
            window.titleContent = new GUIContent("Treasured Scene Tool", Resources.Load<Texture2D>("Treasured_Logo"));
            window.scene = scene;
            window.selection = scene;
            window.Show();

        }

        public TreasuredMap scene;
        SceneView sceneView;

        ToolMode toolMode;
        int selectedTypeIndex;
        Component selection;

        public enum ToolMode
        {
            Create,
            Record
        }

        private void OnEnable()
        {
            ObjectTypeNames = ObjectTypeNames = ObjectTypes.Select(t => ObjectNames.NicifyVariableName(t.Name)).ToArray();
            sceneView = SceneView.lastActiveSceneView;
            ShowSceneViewNotification(new GUIContent("Click on where you want to place the object."), 1.5f);
            SceneView.duringSceneGui -= OnSceneViewGUI;
            SceneView.duringSceneGui += OnSceneViewGUI;
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
            if(selection == null)
                selection = Selection.activeGameObject.GetComponent<TreasuredMap>();

            this.Repaint();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField($"Current Selection");
            using (new EditorGUILayout.HorizontalScope())
            {
                if (selection is TreasuredObject obj)
                {
                    GUIContent rootName = new GUIContent(obj.Map.name);
                    if (GUILayout.Button(rootName, EditorStyles.linkLabel))
                    {
                        Selection.activeObject = obj.Map;
                    }
                    //EditorGUILayout.LabelField(rootName, EditorStyles.linkLabel, GUILayout.Width(EditorStyles.label.CalcSize(rootName).x));
                    EditorGUILayout.LabelField(" > ", Styles.pathLabel, GUILayout.Width(12f));
                }
                EditorGUILayout.LabelField(selection ? selection.name : string.Empty, Styles.pathLabel);
            }
            if (selection == null)
            {
                return;
            }
            if (Selection.objects.Length != 1)
            {
                return;
            }
            switch (selection)
            {
                case TreasuredMap scene:
                    EditorGUILayout.LabelField("Create", EditorStyles.boldLabel);
                    EditorGUI.indentLevel++;
                    EditorGUI.BeginChangeCheck();
                    selectedTypeIndex = GUILayout.SelectionGrid(selectedTypeIndex, ObjectTypeNames, 2, EditorStyles.radioButton);
                    if (EditorGUI.EndChangeCheck())
                    {
                        ShowSceneViewNotification(new GUIContent("Click on where you want to place the object."), 1.5f);
                    }
                    EditorGUI.indentLevel--;
                    break;
                case Hotspot hotspot:
                    EditorGUILayout.LabelField("Controls", EditorStyles.boldLabel);
                    if (GUILayout.Button("Preview"))
                    {
                        hotspot.Camera?.Preview();
                    }
                    if (GUILayout.Button("Record Rotation"))
                    {

                    }
                    if (GUILayout.Button(Styles.snapToGround))
                    {
                        hotspot.SnapToGround();
                    }
                    EditorGUI.indentLevel--;
                    break;
                case Interactable interactable:
                    break;
            }
        }

        void ShowSceneViewNotification(GUIContent notification, double fadeoutWait)
        {
            sceneView?.ShowNotification(notification, fadeoutWait);
            sceneView.Repaint();
        }

        void CreateObject(Type t)
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
            // Place the new game object on floor if collider found.
            Camera camera = sceneView.camera;
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
                SceneView.lastActiveSceneView.LookAt(obj.transform.position, camera.transform.rotation);
            }
            EditorGUIUtility.PingObject(obj);
        }

        void ProcessEvents()
        {
            Event e = Event.current;
            switch (toolMode)
            {
                case ToolMode.Create:
                    if (e.type == EventType.MouseDown && e.button == 0)
                    {
                        if (selectedTypeIndex > -1 && selectedTypeIndex < ObjectTypes.Length)
                        {
                            CreateObject(ObjectTypes[selectedTypeIndex]);
                            sceneView.Repaint();
                        }
                    }
                    break;
                case ToolMode.Record:
                    break;
            }
        }
    }
}
