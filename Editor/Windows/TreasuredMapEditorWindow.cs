using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Treasured.UnitySdk
{
    internal partial class TreasuredMapEditorWindow : EditorWindow
    {
        enum Tabs
        {
            Map = 0,
            Hotspots = 1,
            Interactables = 2
        }

        static class Styles
        {
            public static readonly GUIStyle centeredLabel = new GUIStyle() { wordWrap = true, alignment = TextAnchor.MiddleCenter, richText = true, normal = { textColor = Color.white } };
            
            public static readonly string noAssetSelected = "No map asset selected.\n\n" +
                "To <i><b>Create</b></i> a new Treasured Map\nclick <i><b>Create New</b></i>\n\n" +
                "To <i><b>Load</b></i> an asset in the project\nclick <i><b>Load/From Assets</b></i>\n\n" +
                "To <i><b>Load</b></i> from Json file\nclick <i><b>Load/From Json</b></i>";
            public static readonly string noTargetSelected = "No target is selected";
            public static readonly string defaultAssetNotFound = "Failed to load default asset for Treasured Map Editor. The given asset can not be found.";
        }

        internal static TreasuredMapEditorWindow instance;
        #region Serialized Target & Editor
        TreasuredMapEditorSettings settings;

        SerializedObject serializedMap;
        [NonSerialized]
        TreasuredMap _map;
        TreasuredMap Map
        {
            get => _map;
            set
            {
                _map = value;
                if (_map != null)
                {
                    serializedMap = new SerializedObject(_map);
                    CreateReorderableList(ref hotspotList, serializedMap, serializedMap.FindProperty("hotspots"), _map.Hotspots);
                    CreateReorderableList(ref interactableList, serializedMap, serializedMap.FindProperty("interactables"), _map.Interactables);
                    activeObjectList = hotspotList;
                }
            }
        }

        ReorderableList activeObjectList;
        ReorderableList hotspotList;
        ReorderableList interactableList;

        /// <summary>
        /// Target being inspected. Use InspectedTarget property to set the value to create proper editor.
        /// </summary>
        object _target;
        object Target
        {
            get
            {
                return _target;
            }
            set
            {
                _target = value;
                if (_target == null)
                {
                    _targetEditor = null;
                    return;
                }
                switch (_target)
                {
                    case TreasuredMap map:
                        _targetEditor = Editor.CreateEditor(map);
                        break;
                    case TreasuredMapEditorSettings settings:
                        _targetEditor = Editor.CreateEditor(settings);
                        break;
                    case ObjectBase to:
                        _targetEditor = Editor.CreateEditor(to);
                        break;
                }
                this.Repaint(); // force update the gui
            }
        }
        [NonSerialized]
        Editor _targetEditor;

        private Vector2 objectListScrollPosition;

        [NonSerialized] // prevent script reload resets value but not update active list
        private Tabs selectedTab = Tabs.Map;

        #endregion

        #region Object Links
        private static Dictionary<string, ObjectLink> links = new Dictionary<string, ObjectLink>(); 
        #endregion

        #region Control Settings
        private float overwriteHotspotGroundOffset = 2.5f;
        #endregion

        #region Export Settings
        string folderName;
        #endregion
        void OnEnable()
        {
            LoadSettings();
            if (settings != null && settings.StartUpAsset != null)
            {
                if (EditorUtility.IsPersistent(settings.StartUpAsset))
                {
                    Map = settings.StartUpAsset;
                }
            }
            SceneView.duringSceneGui += OnSceneView;
            this.autoRepaintOnSceneChange = true;
            EditorSceneManager.sceneLoaded += OnSceneLoaded;
            GetAllObjectLinks();
            folderName = SceneManager.GetActiveScene().name;
        }

        void GetAllObjectLinks()
        {
            var links = FindObjectsOfType<ObjectLink>();
            foreach (var link in links)
            {
                if (string.IsNullOrEmpty(link._targetId))
                {
                    Debug.LogWarning($"The ObjectLink component is attached to {link.gameObject.name} but not linked to a Hotspot or Interactable.", link.gameObject);
                }
                if (TreasuredMapEditorWindow.links.TryGetValue(link._targetId, out var linkComponent) || linkComponent == null)
                {
                    TreasuredMapEditorWindow.links[link._targetId] = linkComponent;
                }
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            GetAllObjectLinks();
        }


        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneView;
            EditorSceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void CreateReorderableList(ref ReorderableList reorderableList, SerializedObject serializedObject, SerializedProperty arrayProperty, IList list)
        {
            reorderableList = new ReorderableList(serializedObject, arrayProperty)
            {
                headerHeight = 0,
                list = list
            };
            //reorderableList.drawElementBackgroundCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            //{

            //};
            reorderableList.onAddCallback = (ReorderableList list) =>
            {
                if(activeObjectList.serializedProperty.TryAppendScriptableObject(out var element, out var so))
                {
                    activeObjectList.index = activeObjectList.count - 1;
                    if (so is ObjectBase obj)
                    {
                        obj.SetMap(_map);
                        obj.hitbox = new Hitbox(Vector3.one);
                        Camera camera = SceneView.lastActiveSceneView.camera;
                        if (Physics.Raycast(camera.transform.position, camera.transform.forward, out var hit))
                        {
                            obj.hitbox.center = hit.point + new Vector3(0, obj.hitbox.size.y / 2, 0);
                            if (obj is Hotspot hotspot)
                            {
                                hotspot.cameraTransform.position = hotspot.hitbox.center + new Vector3(0, 2.5f, 0);
                            }
                        };
                        Target = obj;
                    }
                }
            };
            reorderableList.onRemoveCallback = (ReorderableList list) =>
            {
                RemoveElement(list);
            };
            reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                SerializedProperty element = arrayProperty.GetArrayElementAtIndex(index);
                EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, rect.height), element.objectReferenceValue.name);
                // TODO: Add drag and drop scene object and attach link
            };
            reorderableList.onSelectCallback = (ReorderableList list) =>
            {
                Target = (ObjectBase)list.list[list.index];
                SceneView.lastActiveSceneView.Repaint();

                // TODO: Add ability to ping scene object if there is a link.
            };
        }

        void LoadSettings()
        {
            if (settings != null)
            {
                return;
            }
            string[] settingsGuids = AssetDatabase.FindAssets($"t:{typeof(TreasuredMapEditorSettings).Name}");
            if (settingsGuids.Length > 0)
            {
                settings = AssetDatabase.LoadAssetAtPath<TreasuredMapEditorSettings>(AssetDatabase.GUIDToAssetPath(settingsGuids[0]));
            }
        }

        IEnumerable<TreasuredMap> GetAllMapAssets()
        {
            string[] mapAssetGuids = AssetDatabase.FindAssets($"t:{typeof(TreasuredMap).Name}");
            if (mapAssetGuids.Length == 0)
            {
                yield break;
            }
            var mapAssetPaths = mapAssetGuids.Select(guid => AssetDatabase.GUIDToAssetPath(guid));
            foreach (var assetPath in mapAssetPaths)
            {
                yield return AssetDatabase.LoadAssetAtPath<TreasuredMap>(assetPath);
            }
        }

        private void OnGUI()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar, GUILayout.MaxHeight(20), GUILayout.ExpandWidth(true)))
            {
                DrawToolbar();
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                DrawMenu();
                DrawTarget();
            }
        }

        void DrawToolbar()
        {
            if (GUILayout.Button("Create New", EditorStyles.toolbarButton, GUILayout.Width(78)))
            {
                string path = EditorUtility.SaveFilePanel("Select folder", Application.dataPath, "New Treasured Map", "asset");
                if (!string.IsNullOrEmpty(path))
                {
                    TreasuredMap newMap = ScriptableObject.CreateInstance<TreasuredMap>();
                    string relativePath = FileUtil.GetProjectRelativePath(path);
                    AssetDatabase.CreateAsset(newMap, relativePath);
                    AssetDatabase.SaveAssets();
                    if (EditorUtility.IsPersistent(newMap))
                    {
                        this.Map = newMap;
                    }
                }
            }
            if (GUILayout.Button("Load", EditorStyles.toolbarButton, GUILayout.Width(40)))
            {
                GenericMenu menu = new GenericMenu();
                menu.allowDuplicateNames = true;
                var mapAssets = GetAllMapAssets();
                if (mapAssets != null && mapAssets.Any())
                {
                    foreach (var map in mapAssets)
                    {
                        if (map == null)
                        {
                            continue;
                        }
                        if (map.Equals(_map))
                        {
                            menu.AddDisabledItem(new GUIContent($"From Assets/{map.name}"));
                        }
                        else
                        {
                            menu.AddItem(new GUIContent($"From Assets/{map.name}"), false, () =>
                            {
                                this.Target = this.Map = map;
                            });
                        }
                    }
                }
                else
                {
                    menu.AddDisabledItem(new GUIContent("From Assets/No asset available"));
                }
                //menu.AddSeparator("");
                //menu.AddItem(new GUIContent("From Json"), false, () =>
                //{
                //    string path = EditorUtility.OpenFilePanel("Open", Application.dataPath, "json");
                //    if (!string.IsNullOrEmpty(path))
                //    {
                //        TreasuredMap map = ScriptableObject.CreateInstance<TreasuredMap>();
                //        JsonUtility.FromJsonOverwrite(File.ReadAllText(path), map);
                //        AssetDatabase.CreateAsset(map, "Assets/json map.asset");
                //        AssetDatabase.SaveAssets();
                //        //throw new NotImplementedException();
                //    }
                //});
                menu.ShowAsContext();
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(Icons.editorSettings, EditorStyles.label))
            {
                LoadSettings();
                if (settings == null)
                {
                    // Create new settings if no setting found in project.
                    string path = EditorUtility.SaveFilePanel("Save Settings", Application.dataPath, "Treasured Map Editor Settings", "asset");
                    if (!string.IsNullOrEmpty(path))
                    {
                        if (!path.StartsWith(Application.dataPath))
                        {
                            Debug.LogError("Selected folder is not under Assets folder.");
                            return;
                        }
                        TreasuredMapEditorSettings settings = ScriptableObject.CreateInstance<TreasuredMapEditorSettings>();
                        AssetDatabase.CreateAsset(settings, FileUtil.GetProjectRelativePath(path));
                        AssetDatabase.SaveAssets();
                        if (EditorUtility.IsPersistent(settings))
                        {
                            Target = settings;
                        }
                    }
                }
                else
                {
                    Target = settings;
                }
            }
        }

        void DrawMenu()
        {
            using (new EditorGUILayout.VerticalScope("box", GUILayout.Width(200), GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true)))
            {
                if (!EditorUtility.IsPersistent(_map)|| serializedMap == null)
                {
                    GUILayout.Label(Styles.noAssetSelected, Styles.centeredLabel, GUILayout.Width(200), GUILayout.ExpandHeight(true));
                }
                else
                {
                    EditorGUI.BeginChangeCheck();
                    selectedTab = (Tabs)GUILayout.SelectionGrid((int)selectedTab, Enum.GetNames(typeof(Tabs)), 3, EditorStyles.toolbarButton);
                    if (EditorGUI.EndChangeCheck())
                    {
                        switch (selectedTab)
                        {
                            case Tabs.Map:
                                Target = _map;
                                break;
                            case Tabs.Hotspots:
                                hotspotList.index = -1;
                                activeObjectList = hotspotList;
                                Target = null;
                                break;
                            case Tabs.Interactables:
                                interactableList.index = -1;
                                activeObjectList = interactableList;
                                Target = null;
                                break;
                        }
                        SceneView.lastActiveSceneView.Repaint();
                    }
                    if (selectedTab == Tabs.Hotspots)
                    {
                        EditorGUILayout.LabelField(new GUIContent("Overwrites"), EditorStyles.boldLabel);
                        EditorGUI.indentLevel++;
                        EditorGUI.BeginChangeCheck();
                        overwriteHotspotGroundOffset = EditorGUILayout.Slider(new GUIContent("Ground Offset", "Place ALL hotspots above the ground by the amount specified."), overwriteHotspotGroundOffset, 0, 99999);
                        if (EditorGUI.EndChangeCheck())
                        {
                            foreach (var hotspot in _map.Hotspots)
                            {
                                if (Physics.Raycast(hotspot.cameraTransform.position, hotspot.cameraTransform.position - hotspot.cameraTransform.position + Vector3.down, out var hit))
                                {
                                    hotspot.cameraTransform.position.y = hit.point.y + overwriteHotspotGroundOffset;
                                }
                            }
                        }
                        EditorGUI.indentLevel--;
                    }
                    using (var scope = new EditorGUILayout.ScrollViewScope(objectListScrollPosition, GUILayout.ExpandHeight(false)))
                    {
                        objectListScrollPosition = scope.scrollPosition;
                        switch (selectedTab)
                        {
                            case Tabs.Map:
                                DrawMapControls();
                                break;
                            case Tabs.Hotspots:
                            case Tabs.Interactables:
                                activeObjectList.DoLayoutList();
                                break;
                        }
                    }
                }
            }
        }

        void DrawMapControls()
        {
            using(var scope = new EditorGUILayout.VerticalScope())
            {
                float previousLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 96;
                EditorGUI.BeginChangeCheck();
                DrawControlGroup("Map Settings", () =>
                {
                    //EditorGUIUtility.labelWidth = scope.rect.width - 20; // <- ensure toggle box is always on the right
                    _map.loop = EditorGUILayout.Toggle("Loop", _map.loop);                                                                                                                                                                                                       
                });
                if (EditorGUI.EndChangeCheck())
                {
                    SceneView.lastActiveSceneView.Repaint();
                }
                DrawControlGroup("Export Settings", () =>
                {
                    folderName = EditorGUILayout.TextField(new GUIContent("Folder Name"), folderName);
                    _map.format = (Format)EditorGUILayout.EnumPopup(new GUIContent("Format"), _map.format);
                    _map.quality = (Quality)EditorGUILayout.EnumPopup(new GUIContent("Quality"), _map.quality);
                    using(new EditorGUI.DisabledGroupScope(string.IsNullOrEmpty(folderName)))
                    {
                        if (GUILayout.Button("Export"))
                        {
                            TreasuredMapExporter.Export(_map, folderName);
                        }
                    }
                });
                EditorGUIUtility.labelWidth = previousLabelWidth;
            }
        }
        
        private void DrawControlGroup(string name, Action drawControl)
        {
            EditorGUILayout.LabelField(name, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            drawControl.Invoke();
            EditorGUI.indentLevel--;
        }

        void RemoveElement(ReorderableList list)
        {
            int indexToRemove = list.index;
            list.serializedProperty.RemoveElementAtIndex(indexToRemove);
            if (list.serializedProperty.arraySize > 0)
            {
                list.index = Mathf.Min(indexToRemove, list.serializedProperty.arraySize - 1);
                Target = (ObjectBase)list.serializedProperty.GetArrayElementAtIndex(list.index).objectReferenceValue;
            }
            else
            {
                Target = null;
            }
        }


        //void DrawMenuItems()
        //{
        //    Event e = Event.current;
        //    using (new EditorGUILayout.VerticalScope())
        //    {
        //        DrawMenuItem("Map", Map, e);
        //        //DrawMenuItem("Map/Settings", settings, e);
        //    }
        //}

        //void DrawMenuItem(string path, object target, Event e)
        //{
        //    // TODO : Create Path with foldout
        //    EditorGUILayout.LabelField(new GUIContent(path));
        //    Rect lastRect = GUILayoutUtility.GetLastRect();
        //    EditorGUIUtility.AddCursorRect(lastRect, MouseCursor.Link);
        //    if (lastRect.Contains(e.mousePosition) && (e.type == EventType.MouseDown || e.type == EventType.MouseUp) && e.button == 0)
        //    {
        //        Target = target;
        //    }
        //}

        void DrawTarget()
        {
            using (new EditorGUILayout.VerticalScope("box", GUILayout.ExpandHeight(true)))
            {
                if (_targetEditor == null)
                {
                    EditorGUILayout.LabelField(Styles.noTargetSelected, Styles.centeredLabel, GUILayout.ExpandHeight(true));
                }
                else
                {
                    _targetEditor.OnInspectorGUI();
                }
            }
            
        }

        private void SaveAsJson()
        {
            if (Map == null)
            {
                throw new NullReferenceException("Map is null.");
            }
            if (!EditorUtility.IsPersistent(Map))
            {
                throw new InvalidOperationException("Asset not saved on disk.");
            }
            string path = EditorUtility.SaveFilePanel("Save as", Application.dataPath, "data", "json");
            if (!string.IsNullOrEmpty(path)) // check if operation is canceled
            {
                string json = JsonUtility.ToJson(Map, true);
                File.WriteAllText(path, json);
                Application.OpenURL(path);
            }
        }
    }
}
