using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using System;
using Treasured.SDK;
using Treasured.UnitySdk.Editor;

namespace Treasured.SDKEditor
{
    public class TreasuredDataEditorWindow : EditorWindow
    {
        private const string DefaultSettingsDirectory = "Assets/Treasured Toolkit/";
        private const string HotspotsPropertyPath = "_hotspots";
        private const string InteractablesPropertyPath = "_interactables";

        private static readonly string[] PropertySkipList = new string[] { "m_Script", HotspotsPropertyPath, InteractablesPropertyPath };
        private static string[] TabList = new string[] { "Metadata", "Hotspots", "Interactables" };
        #region Window Properties
        private static readonly Vector2 MinSize = new Vector2(400, 600);

        //[MenuItem("Tools/Treasured/Editor")]
        static TreasuredDataEditorWindow OpenWindow()
        {
            var window = GetWindow<TreasuredDataEditorWindow>();
            //window.minSize = MinSize;
            window.Show();
            return window;
        }
        #endregion

        #region Editor Properties
        [SerializeField]
        private TreasuredDataEditorSettings _editorSettings;
        [SerializeField]
        private TreasuredData _data;
        [SerializeField]
        private SerializedObject _serializedObject;
        [SerializeField]
        private SerializedObject _serializedSettings;
        [SerializeField]
        private SerializedObject _serializedData;
        [SerializeField]
        private SerializedProperty _currentProperty;
        [SerializeField]
        private bool _isEditingMode;
        [SerializeField]
        private int _selectedTab;

        private ReorderableList _hotspotList;
        private ReorderableList _interactableList;

        private Vector2 _scrollPosition;

        private bool _interactableToggleAll = false;

        private TreasuredData Data
        {
            get
            {
                return _data;
            }
            set
            {
                if (value == null)
                {
                    Debug.LogWarning("Unable to load null data.");
                    return;
                }
                _data = value;
                UpdateMenuItems();
                RebuildObjectIds();
                 _serializedData = new SerializedObject(_data);
                CreateReorderableList();
            }
        }

        internal static void Open(TreasuredData data)
        {
            var window = OpenWindow();
            window.Init(data);
        }

        private IEnumerable<string> _dataAssetPaths;
        #endregion

        #region Treasured Data
        /// <summary>
        /// Current editing data.
        /// </summary>
        public static readonly Dictionary<string, string> ObjectIds = new Dictionary<string, string>();
        private void RebuildObjectIds()
        {
            ObjectIds.Clear();
            foreach (var hotspot in _data.Hotspots)
            {
                string id = hotspot.Id;
                if (!string.IsNullOrEmpty(id) && !ObjectIds.ContainsKey(id))
                {
                    ObjectIds[id] = $"Hotspots/{hotspot.Name} | {id}";
                }
                if (hotspot.Hitbox.Size == Vector3.zero)
                {
                    hotspot.Hitbox.Size = Vector3.one;
                }
            }
            foreach (var interactable in _data.Interactables)
            {
                string id = interactable.Id;
                if (!string.IsNullOrEmpty(id) && !ObjectIds.ContainsKey(id))
                {
                    ObjectIds[id] = $"Interactables/{interactable.Name} | {id}";
                }
                if (interactable.Hitbox.Size == Vector3.zero)
                {
                    interactable.Hitbox.Size = Vector3.one;
                }
            }
        }
        #endregion
        private void OnEnable()
        {
            Init(null);
        }

        private void Init(TreasuredData data)
        {
            string[] settingsGuids = AssetDatabase.FindAssets("t: TreasuredDataEditorSettings");
            if (settingsGuids.Length > 0)
            {
                _editorSettings = AssetDatabase.LoadAssetAtPath<TreasuredDataEditorSettings>(AssetDatabase.GUIDToAssetPath(settingsGuids[0]));
            }
            if (_editorSettings != null)
            {
                _serializedSettings = new SerializedObject(_editorSettings);
                if (data == null)
                {
                    if (!string.IsNullOrEmpty(_editorSettings.StartUpDataAssetGUID))
                    {
                        Data = AssetDatabase.LoadAssetAtPath<TreasuredData>(AssetDatabase.GUIDToAssetPath(_editorSettings.StartUpDataAssetGUID));
                    }
                }
                else
                {
                    Data = data;
                }
                SetTarget(_serializedData);
            }
        }

        private void OnGUI()
        {
            if (_data != null && Event.current.type == EventType.Repaint && ObjectIds.Count != _data.Hotspots.Count + _data.Interactables.Count)
            {
                RebuildObjectIds();
            }
            _serializedObject?.Update();
            using (new EditorGUILayout.VerticalScope())
            {
                using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar, GUILayout.ExpandWidth(true), GUILayout.Height(20)))
                {
                    DrawToolbar();
                }
                using (new EditorGUILayout.VerticalScope(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        DrawMenu();
                        using (new EditorGUILayout.VerticalScope(GUILayout.ExpandWidth(true)))
                        {
                            DrawTarget();
                        }
                    }
                }
                using (new EditorGUILayout.HorizontalScope(GUILayout.ExpandWidth(true)))
                {
                    GUILayout.FlexibleSpace();
                    using (new EditorGUI.DisabledGroupScope(true))
                    {
                        EditorGUILayout.LabelField($"v{TreasuredData.Version}", GUILayout.Width(45));
                    };
                }
            }
            if (GUI.changed)
            {
                _serializedObject?.ApplyModifiedProperties();
            }
        }

        private void DrawMetaData()
        {
            SerializedProperty nameProp = _serializedObject.FindProperty("_name");
            SerializedProperty loopProp = _serializedObject.FindProperty("_loop");
            SerializedProperty formatProp = _serializedObject.FindProperty("_format");
            SerializedProperty qualityProp = _serializedObject.FindProperty("_quality");

            EditorGUILayout.PropertyField(nameProp);
            if (string.IsNullOrEmpty(nameProp.stringValue))
            {
                string sceneName = EditorSceneManager.GetActiveScene().name;
                EditorGUILayout.HelpBox($"The default output folder name will be '{sceneName}'.", MessageType.Warning);
            }
            EditorGUILayout.PropertyField(loopProp);
            EditorGUILayout.PropertyField(formatProp);
            EditorGUILayout.PropertyField(qualityProp);
            if (qualityProp.enumValueIndex == 3)
            {
                EditorGUILayout.HelpBox("Use with caution!\n" +
                    "Ultra setting will use a lot of memory due to a bug with Unity.\n" +
                    "The memory is unlikely to be released until entering or exiting play mode and upon assembly reloaded.", MessageType.Warning);
            }
        }

        private void DrawMenu()
        {
            using (new EditorGUILayout.VerticalScope("box", GUILayout.Width(180), GUILayout.ExpandHeight(true)))
            {
                if (_data)
                {
                    _isEditingMode = EditorGUILayout.ToggleLeft("Editing Mode", _isEditingMode);
                    EditorGUI.BeginChangeCheck();
                    _selectedTab = GUILayout.SelectionGrid(_selectedTab, TabList, 1, EditorStyles.toolbarButton);
                    if (EditorGUI.EndChangeCheck())
                    {
                        switch (_selectedTab)
                        {
                            case 0:
                                SetTarget(_serializedData);
                                break;
                            case 1:
                                SetTarget(_serializedData, HotspotsPropertyPath, _hotspotList.index);
                                break;
                            case 2:
                                SetTarget(_serializedData, InteractablesPropertyPath, _interactableList.index);
                                break;
                        }
                    }
                    using (var scope = new EditorGUILayout.ScrollViewScope(_scrollPosition))
                    {
                        _scrollPosition = scope.scrollPosition;
                        switch (_selectedTab)
                        {
                            case 1:
                                _hotspotList?.DoLayoutList();
                                break;
                            case 2:
                                _interactableList?.DoLayoutList();
                                break;
                        }
                    }
                    if (_selectedTab == 1 || _selectedTab == 2)
                    {
                        Rect rect = GUILayoutUtility.GetLastRect();
                        SerializedProperty property = _selectedTab == 1 ? _serializedData.FindProperty(HotspotsPropertyPath) : _serializedData.FindProperty(InteractablesPropertyPath);
                        CustomEditorGUILayout.CreateDropZone(rect, new GUIContent(""), (objects) =>
                        {
                            foreach (var obj in objects)
                            {
                                GameObject go = obj as GameObject;
                                if (!go)
                                {
                                    continue;
                                }
                                EditorUtility.SetDirty(go);
                                property.InsertArrayElementAtIndex(property.arraySize);
                                SerializedProperty p = property.GetArrayElementAtIndex(property.arraySize - 1);
                                p.FindPropertyRelative("_name").stringValue = obj.name;
                                p.FindPropertyRelative("_id").stringValue = Guid.NewGuid().ToString();
                                p.FindPropertyRelative("_transform._position").vector3Value = go.transform.position;
                                p.FindPropertyRelative("_transform._rotation").vector3Value = go.transform.rotation.eulerAngles;
                                if (go.TryGetComponent<Collider>(out var collider))
                                {
                                    p.FindPropertyRelative("_hitbox._center").vector3Value = collider.bounds.center;
                                    p.FindPropertyRelative("_hitbox._size").vector3Value = collider.bounds.size;
                                }
                                property.serializedObject.ApplyModifiedProperties();
                                property.serializedObject.Update();
                                if (_selectedTab == 1)
                                {
                                    hotspotList.Add(new ListItem()
                                    {
                                        Export = true,
                                        Object = _data.Hotspots[property.arraySize - 1],
                                        ElementIndex = property.arraySize - 1
                                    });
                                }
                                else
                                {
                                    interactableList.Add(new ListItem()
                                    {
                                        Export = true,
                                        Object = _data.Interactables[property.arraySize - 1],
                                        ElementIndex = property.arraySize - 1
                                    });
                                }
                            }
                            if (_selectedTab == 1)
                            {
                                _hotspotList.index = property.arraySize - 1;
                            }
                            else
                            {
                                _interactableList.index = property.arraySize - 1;
                            }
                            UpdateMenuItems();
                        });
                    }
                }
                else
                {
                    GUILayout.Box("No data selected\nClick 'Create New' to create an new asset\nOR\nClick 'Load' to load from existing asset", new GUIStyle("label") { alignment = TextAnchor.MiddleCenter }, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                }
            }
        }

        private void DrawToolbar()
        {
            if (GUILayout.Button("Create New", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                ToolbarCreateNew();
            }
            if (GUILayout.Button("Load", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                ToolbarLoad();
            }
            if (GUILayout.Button("Export", EditorStyles.toolbarButton, GUILayout.Width(70)))
            {

            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(EditorGUIUtility.TrIconContent("Settings", "Editor Settings"), EditorStyles.label, GUILayout.Width(20)))
            {
                SetTarget(_serializedSettings);
            }
            
        }

        private void ToolbarLoad()
        {
            string[] dataAssetGuids = AssetDatabase.FindAssets("t: TreasuredData");
            _dataAssetPaths = dataAssetGuids.Select(guid => AssetDatabase.GUIDToAssetPath(guid));
            GenericMenu menu = new GenericMenu();
            foreach (var path in _dataAssetPaths)
            {
                menu.AddItem(new GUIContent(path), false, () =>
                {
                    Data = AssetDatabase.LoadAssetAtPath<TreasuredData>(path);
                });
            }
            menu.ShowAsContext();
        }

        private void ToolbarCreateNew()
        {
            TreasuredData data = Utility.ShowCreateAssetPanel<TreasuredData>("Treasured Data", DefaultSettingsDirectory);
            if (data == null)
            {
                Debug.LogWarning("Cancel to create Treasured Data.");
            }
            else
            {
                Data = data;
            }
        }

        private void DrawTarget()
        {
            if (_currentProperty == null)
            {
                GUILayout.Box("No target is selected", new GUIStyle("label") { alignment = TextAnchor.MiddleCenter }, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                return;
            }
            _currentProperty.serializedObject.Update();
            if (string.IsNullOrEmpty(_currentProperty.propertyPath))
            {
                SerializedProperty iterator = _currentProperty.Copy();
                bool enterChildren = true;
                while (iterator.NextVisible(enterChildren))
                {
                    enterChildren = false;
                    if (PropertySkipList.Contains(iterator.name))
                    {
                        continue;
                    }
                    EditorGUILayout.PropertyField(iterator);
                }
            }
            else
            {
                EditorGUILayout.PropertyField(_currentProperty);
            }
            _currentProperty.serializedObject.ApplyModifiedProperties();
        }

        private void SetTarget(SerializedObject obj, string relativePath = "", int arrayIndex = -1)
        {
            if (obj == null)
            {
                _serializedObject = null;
                _currentProperty = null;
                return;
            }
            _serializedObject = obj;
            if (string.IsNullOrEmpty(relativePath) || arrayIndex == -1)
            {
                _currentProperty = null;
            }
            else
            {
                _currentProperty = _serializedObject.FindProperty(relativePath);
                if (_currentProperty.isArray)
                {
                    if (arrayIndex != -1 && arrayIndex < _currentProperty.arraySize)
                    {
                        _currentProperty = _currentProperty.GetArrayElementAtIndex(arrayIndex);
                    }
                }
            }
        }
        List<ListItem> hotspotList;
        List<ListItem> interactableList;
        private bool _hotspotToggleAll;

        private void CreateReorderableList()
        {
            hotspotList = new List<ListItem>();
            for (int i = 0; i < _data.Hotspots.Count; i++)
            {
                hotspotList.Add(new ListItem()
                {
                    Export = true,
                    Object = _data.Hotspots[i],
                    ElementIndex = i
                });
            }
            interactableList = new List<ListItem>();
            for (int i = 0; i < _data.Interactables.Count; i++)
            {
                interactableList.Add(new ListItem()
                {
                    Export = true,
                    Object = _data.Interactables[i],
                    ElementIndex = i
                });
            }
            _hotspotList = new ReorderableList(hotspotList, typeof(ListItem))
            {
                displayAdd = false,
                displayRemove = false,
                headerHeight = 0,
                drawHeaderCallback = (Rect rect) =>
                {
                    EditorGUI.BeginChangeCheck();
                    _hotspotToggleAll = EditorGUI.Toggle(new Rect(rect.x, rect.y, 20, 20), _hotspotToggleAll);
                    if (EditorGUI.EndChangeCheck())
                    {
                        foreach (var item in hotspotList)
                        {
                            item.Export = _hotspotToggleAll;
                        }
                    }
                    EditorGUI.LabelField(new Rect(rect.x + 20, rect.y, rect.xMax - 20, 20), new GUIContent($"Count: {_data.Hotspots.Count}"));
                    if (GUI.Button(new Rect(rect.xMax - 20, rect.y, 20, 20), EditorGUIUtility.TrIconContent("Toolbar Plus", "Create New"), EditorStyles.toolbarButton))
                    {
                        SerializedProperty newObj = InsertNewTreasuredObject(HotspotsPropertyPath, _selectedTab == 1 ? "Hotspot" : "Object", Vector3.zero, Vector3.zero, Vector3.zero, Vector3.one);
                        interactableList.Add(new ListItem()
                        {
                            Export = true,
                            Object = _data.Hotspots[_data.Interactables.Count - 1],
                            ElementIndex = _data.Hotspots.Count - 1
                        });
                        UpdateMenuItems();
                    }
                },
                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    if (index >= hotspotList.Count)
                    {
                        return;
                    }
                    TreasuredObject obj = hotspotList[index].Object;
                    hotspotList[index].Export = EditorGUI.ToggleLeft(new Rect(rect.x, rect.y, 18, 20), obj.Name, hotspotList[index].Export);
                    EditorGUI.LabelField(new Rect(rect.x + 20, rect.y, rect.width - 40, 20), new GUIContent(obj.Name));
                    if (GUI.Button(new Rect(rect.xMax - 20, rect.y, 20, 20), EditorGUIUtility.TrIconContent("Toolbar Minus", "Delete Selected"), EditorStyles.label))
                    {
                        SerializedProperty array = _serializedData.FindProperty(HotspotsPropertyPath);
                        array.DeleteArrayElementAtIndex(index);
                        interactableList.RemoveAt(index);
                        array.serializedObject.ApplyModifiedProperties();
                        UpdateMenuItems();
                    }
                },
                onSelectCallback = (ReorderableList list) =>
                {
                    SetTarget(_serializedData, HotspotsPropertyPath, hotspotList[list.index].ElementIndex);
                }
            };
            _interactableList = new ReorderableList(interactableList, typeof(ListItem))
            {
                displayAdd = false,
                displayRemove = false,
                drawHeaderCallback = (Rect rect) =>
                {
                    EditorGUI.BeginChangeCheck();
                    _interactableToggleAll = EditorGUI.Toggle(new Rect(rect.x, rect.y, 20, 20), _interactableToggleAll);
                    if (EditorGUI.EndChangeCheck())
                    {
                        foreach (var item in interactableList)
                        {
                            item.Export = _interactableToggleAll;
                        }
                    }
                    EditorGUI.LabelField(new Rect(rect.x + 20, rect.y, rect.xMax - 20, 20), new GUIContent($"Count: {_data.Interactables.Count}"));
                    if (GUI.Button(new Rect(rect.xMax - 20, rect.y, 20, 20), EditorGUIUtility.TrIconContent("Toolbar Plus", "Create New"), EditorStyles.toolbarButton))
                    {
                        SerializedProperty newObj = InsertNewTreasuredObject(InteractablesPropertyPath, _selectedTab == 1 ? "Hotspot" : "Object", Vector3.zero, Vector3.zero, Vector3.zero, Vector3.one);
                        interactableList.Add(new ListItem()
                        {
                            Export = true,
                            Object = _data.Interactables[_data.Interactables.Count - 1],
                            ElementIndex = _data.Interactables.Count - 1
                        });
                        UpdateMenuItems();
                    }
                },
                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    if (index >= interactableList.Count)
                    {
                        return;
                    }
                    TreasuredObject obj = interactableList[index].Object;
                    interactableList[index].Export = EditorGUI.ToggleLeft(new Rect(rect.x, rect.y, 18, 20), obj.Name, interactableList[index].Export);
                    EditorGUI.LabelField(new Rect(rect.x + 20, rect.y, rect.width - 40, 20), new GUIContent(obj.Name));
                    if (GUI.Button(new Rect(rect.xMax - 20, rect.y, 20, 20), EditorGUIUtility.TrIconContent("Toolbar Minus", "Delete Selected"), EditorStyles.label))
                    {
                        SerializedProperty array = _serializedData.FindProperty(InteractablesPropertyPath);
                        array.DeleteArrayElementAtIndex(index);
                        interactableList.RemoveAt(index);
                        array.serializedObject.ApplyModifiedProperties();
                        UpdateMenuItems();
                    }
                },
                onSelectCallback = (ReorderableList list) =>
                {
                    SetTarget(_serializedData, InteractablesPropertyPath, interactableList[list.index].ElementIndex);
                },
                //onReorderCallbackWithDetails = (ReorderableList list, int oldIndex, int newIndex) =>
                //{
                //    MoveElement(InteractablesPropertyPath, oldIndex, newIndex);
                //}
            };
        }

        private void UpdateMenuItems()
        {
            if (_data == null)
            {
                return;
            }
            TabList[1] = $"Hotspots ({_data.Hotspots.Count})";
            TabList[2] = $"Interactables ({_data.Interactables.Count})";
        }

        private SerializedProperty InsertNewTreasuredObject(string propertyPath, string namePrefix, Vector3 position, Vector3 rotation, Vector3 hitBoxCenter, Vector3 hitBoxSize)
        {
            SerializedProperty array = _serializedData.FindProperty(propertyPath);
            if(array == null || !array.isArray)
            {
                return null;
            }
            array.InsertArrayElementAtIndex(array.arraySize);
            SerializedProperty newElement = array.GetArrayElementAtIndex(array.arraySize - 1);
            // insert a new element will copy the data from the previous element
            newElement.FindPropertyRelative("_id").stringValue = Guid.NewGuid().ToString();
            newElement.FindPropertyRelative("_name").stringValue = $"{namePrefix} {array.arraySize}";
            newElement.FindPropertyRelative("_description").stringValue = "";
            newElement.FindPropertyRelative("_transform._position").vector3Value = position;
            newElement.FindPropertyRelative("_transform._rotation").vector3Value = rotation;
            newElement.FindPropertyRelative("_hitbox._center").vector3Value = hitBoxCenter;
            newElement.FindPropertyRelative("_hitbox._size").vector3Value = hitBoxSize;
            newElement.FindPropertyRelative("_onSelected").arraySize = 0;
            newElement.serializedObject.ApplyModifiedProperties();
            return newElement;
        }

        private void MoveElement(string propertyName, int oldIndex, int newIndex)
        {
            SerializedProperty property = _serializedData.FindProperty(propertyName);
            if (property != null && property.isArray)
            {
                property.MoveArrayElement(oldIndex, newIndex);
            }
            property.serializedObject.ApplyModifiedProperties();
        }

        private SerializedProperty GetParent(SerializedProperty element)
        {
            string path = element.propertyPath.Substring(0, element.propertyPath.LastIndexOf('.'));
            SerializedProperty p = element.serializedObject.FindProperty(path);
            return p;
        }
    }
}
