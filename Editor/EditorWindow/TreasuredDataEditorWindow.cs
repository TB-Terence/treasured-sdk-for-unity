using UnityEngine;
using UnityEditor;
using Treasured.SDK;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEditorInternal;

namespace Treasured.SDKEditor
{
    public class TreasuredDataEditorWindow : EditorWindow
    {
        private const string DefaultSettingsDirectory = "Assets/Treasured Toolkit/";
        private static readonly string[] PropertySkipList = new string[] { "m_Script", "_interactables", "_hotspots" };
        private static readonly string[] TabList = new string[] { "Metadata", "Hotspots", "Interactables" };
        #region Window Properties
        private static readonly Vector2 MinSize = new Vector2(400, 600);

        [MenuItem("Tools/Treasured/Editor")]
        static void OpenWindow()
        {
            var window = GetWindow<TreasuredDataEditorWindow>();
            //window.minSize = MinSize;
            window.Show();
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
        private SerializedObject _serializedData;
        [SerializeField]
        private bool _isEditingMode;
        [SerializeField]
        private int _selectedTab;

        private string _selectedPropertyPath;

        private ReorderableList _hotspotList;
        private ReorderableList _interactableList;

        private bool _drawObject = false;

        public TreasuredData Data
        {
            get
            {
                return _data;
            }
            private set
            {
                if (value == null)
                {
                    Debug.LogWarning("Unable to load null data.");
                    return;
                }
                _data = value;
                _serializedData = new SerializedObject(_data);
                CreateReorderableList();
            }
        }

        private IEnumerable<string> _dataAssetPaths;
        #endregion

        private void OnEnable()
        {
            string[] settingsGuids = AssetDatabase.FindAssets("t: TreasuredDataEditorSettings");
            if (settingsGuids.Length > 0)
            {
                _editorSettings = AssetDatabase.LoadAssetAtPath<TreasuredDataEditorSettings>(AssetDatabase.GUIDToAssetPath(settingsGuids[0]));
            }
            _serializedObject = new SerializedObject(this);
        }

        private void OnGUI()
        {
            _serializedObject.Update();
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
                        DrawTarget();
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
            _serializedObject.Update();
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
                                _selectedPropertyPath = nameof(_data);
                                _drawObject = false;
                                break;
                            default:
                                _selectedPropertyPath = string.Empty;
                                _drawObject = false;
                                break;
                        }
                    }
                    using (new EditorGUILayout.VerticalScope())
                    {
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
                }
                else
                {
                    GUILayout.Box("No data selected\nClick 'Create New' to create an new asset\nOR\nClick 'Load' to load from existing asset", new GUIStyle("label") { alignment = TextAnchor.MiddleCenter }, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                }
            }
        }

        private void DrawTarget()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.ExpandWidth(true)))
            {
                DrawTarget(!_drawObject ? _serializedObject.FindProperty(_selectedPropertyPath) : _serializedData.FindProperty(_selectedPropertyPath));
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
                _selectedPropertyPath = nameof(_editorSettings);
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

        private void DrawTarget(SerializedProperty property)
        {
            if (property == null)
            {
                GUILayout.Box("No target is selected", new GUIStyle("label") { alignment = TextAnchor.MiddleCenter }, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                return;
            }
            if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                if (property.objectReferenceValue == null)
                {
                    switch (property.propertyPath)
                    {
                        case nameof(_editorSettings):
                            using (var scope = new EditorGUILayout.VerticalScope(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
                            {
                                if (GUILayout.Button(new GUIContent("Create Settings")))
                                {
                                    _editorSettings = Utility.ShowCreateAssetPanel<TreasuredDataEditorSettings>("Treasured Data Editor Settings", DefaultSettingsDirectory);
                                    if (_editorSettings == null)
                                    {
                                        Debug.LogWarning("Cancel create settings for Treasured Data Editor.");
                                    }
                                }
                            }
                            break;
                        case nameof(_data):
                            break;
                    }
                    return;
                }
                SerializedObject obj = new SerializedObject(property.objectReferenceValue);
                obj.Update();
                SerializedProperty iterator = obj.GetIterator();
                iterator.Next(true);
                while (iterator.NextVisible(false))
                {
                    if (PropertySkipList.Contains(iterator.name))
                    {
                        continue;
                    }
                    EditorGUILayout.PropertyField(iterator);
                }
                obj.ApplyModifiedProperties();
            }
            else if (property.propertyType == SerializedPropertyType.Generic)
            {
                property.serializedObject.Update();
                SerializedProperty iterator = property.Copy();
                iterator.Next(true);
                while (iterator.NextVisible(false))
                {
                    if (PropertySkipList.Contains(iterator.name))
                    {
                        continue;
                    }
                    EditorGUILayout.PropertyField(iterator);
                }
                property.serializedObject.ApplyModifiedProperties();
            }
        }

        private void CreateReorderableList()
        {
            List<ListItem> hotspotList = new List<ListItem>();
            for (int i = 0; i < _data.Hotspots.Count; i++)
            {
                hotspotList.Add(new ListItem()
                {
                    Export = true,
                    Object = _data.Hotspots[i],
                    PropertyPath = $"_hotspots.Array.data[{i}]"
                });
            }
            List<ListItem> interactableList = new List<ListItem>();
            for (int i = 0; i < _data.Interactables.Count; i++)
            {
                interactableList.Add(new ListItem()
                {
                    Export = true,
                    Object = _data.Interactables[i],
                    PropertyPath = $"_interactables.Array.data[{i}]"
                });
            }
            _hotspotList = new ReorderableList(hotspotList, typeof(ListItem))
            {
                displayAdd = false,
                displayRemove = false,
                drawHeaderCallback = (Rect rect) =>
                {
                    EditorGUI.LabelField(rect, new GUIContent("Hotspots"));
                },
                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    TreasuredObject obj = hotspotList[index].Object;
                    hotspotList[index].Export = EditorGUI.ToggleLeft(new Rect(rect.x, rect.y, 18, 20), obj.Name, hotspotList[index].Export);
                    EditorGUI.LabelField(new Rect(rect.x + 20, rect.y, rect.xMax - 20, 20), new GUIContent(obj.Name));
                },
                onSelectCallback = (ReorderableList list) =>
                {
                    _selectedPropertyPath = hotspotList[list.index].PropertyPath;
                    _drawObject = true;
                }
            };
            _interactableList = new ReorderableList(interactableList, typeof(ListItem))
            {
                displayAdd = false,
                displayRemove = false,
                drawHeaderCallback = (Rect rect) =>
                {
                    EditorGUI.LabelField(rect, new GUIContent("Interactables"));
                },
                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    TreasuredObject obj = interactableList[index].Object;
                    interactableList[index].Export = EditorGUI.ToggleLeft(new Rect(rect.x, rect.y, 18, 20), obj.Name, interactableList[index].Export);
                    EditorGUI.LabelField(new Rect(rect.x + 20, rect.y, rect.xMax - 20, 20), new GUIContent(obj.Name));
                },
                onSelectCallback = (ReorderableList list) =>
                {
                    _selectedPropertyPath = interactableList[list.index].PropertyPath;
                    _drawObject = true;
                }
            };
        }
    }
}
