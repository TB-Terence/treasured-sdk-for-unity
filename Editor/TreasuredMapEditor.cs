using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Treasured.UnitySdk.Editor
{
    [CustomEditor(typeof(TreasuredMap))]
    internal sealed partial class TreasuredMapEditor : TreasuredEditor<TreasuredMap>
    {
        private enum GroupToggleState
        {
            None,
            All,
            Mixed
        }

        private bool _showMapSettings = true;
        private SerializedProperty _interactableLayer;

        private bool _showInfo = true;

        private bool _showManagementTabs = true;
        private string[] _objectManagementTabs = new string[2] { "Hotspot Management", "Interactable Management" };

        private bool _showAll;
        private List<Hotspot> _hotspots;
        private bool _showHotspotList = true;
        private bool _exportAllHotspots = true;
        private GroupToggleState _hotspotsGroupToggleState = GroupToggleState.All;

        private List<Interactable> _interactables;
        private bool _showInteractableList = true;
        private bool _exportAllInteractables = true;
        private GroupToggleState _interactablesGroupToggleState = GroupToggleState.All;

        private bool _showExportSettings = true;

        private SerializedProperty _id;
        private SerializedProperty _title;
        private SerializedProperty _description;
        private SerializedProperty _loop;

        private TreasuredObject _currentEditingObject = null;
        private int _selectedObjectTab;

        #region Hotspot Management
        private float _hotspotGroundOffset = 2;
        #endregion

        #region Export Settings
        private SerializedProperty _format;
        private SerializedProperty _quality;
        private SerializedProperty _outputDirectory;
        private bool _showInExplorer = true;
        #endregion

        protected override void Init()
        {
            TreasuredMapEditorUtility.RefreshIds(Target);
            Target.transform.hideFlags = HideFlags.HideInInspector;
            _hotspots = Target.gameObject.GetComponentsInChildren<Hotspot>(true).ToList();
            _interactables = Target.gameObject.GetComponentsInChildren<Interactable>(true).ToList();
            Target.Data.GenerateHotspots(_hotspots);
            Target.Data.GenerateInteractables(_interactables);
            Target.Data.Validate();
            InitSerializedProperty();
            Tools.hidden = true;
            _sceneName = SceneManager.GetActiveScene().name;
        }

        private void OnDisable()
        {
            if (Target)
            {
                Target.transform.hideFlags = HideFlags.None;
            }
            Tools.hidden = false;
        }

        private void InitSerializedProperty()
        {
            _interactableLayer = serializedObject.FindProperty(nameof(_interactableLayer));
            _id = serializedObject.FindProperty($"_data.{nameof(_id)}");
            _title = serializedObject.FindProperty($"_data.{nameof(_title)}");
            _description = serializedObject.FindProperty($"_data.{nameof(_description)}");
            _loop = serializedObject.FindProperty($"_data.{nameof(_loop)}");
            _format = serializedObject.FindProperty($"_data.{nameof(_format)}");
            _quality = serializedObject.FindProperty($"_data.{nameof(_quality)}");
            _outputDirectory = serializedObject.FindProperty(nameof(_outputDirectory));
        }


        public override void OnInspectorGUI()
        {
            Styles.Init();
            serializedObject.Update();
            using (new EditorGUILayout.VerticalScope())
            {
                DrawFoldoutGroup(ref _showMapSettings, new GUIContent("Map Settings"), DrawMapSettings);
                DrawFoldoutGroup(ref _showInfo, new GUIContent("Info"), DrawInfo);
                DrawFoldoutGroup(ref _showManagementTabs, new GUIContent("Object Management"), DrawObjectManagement);
                DrawFoldoutGroup(ref _showExportSettings, new GUIContent("Export Settings"), DrawExportSettings);
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawObjectManagement()
        {
            EditorGUI.BeginChangeCheck();
            _showAll = EditorGUILayout.Toggle(new GUIContent("Show Transform Tool for All", "Show transform tool for Hotspots and Interactables if enabled, otherwise only show from selected tab."), _showAll);
            using (new EditorGUILayout.HorizontalScope(Styles.TabBar))
            {
                _selectedObjectTab = GUILayout.SelectionGrid(_selectedObjectTab, _objectManagementTabs, _objectManagementTabs.Length, Styles.TabButton);
            }
            if (EditorGUI.EndChangeCheck())
            {
                SceneView.RepaintAll();
            }
            using (new EditorGUILayout.VerticalScope(Styles.TabPage))
            {
                if (_selectedObjectTab == 0)
                {
                    DrawHotspotManagement();
                }
                else
                {
                    DrawInteractableManagment();
                }
            }
        }

        private void DrawFoldoutGroup(ref bool foldout, GUIContent label, Action action)
        {
            foldout = EditorGUILayout.BeginFoldoutHeaderGroup(foldout, label);
            EditorGUI.indentLevel++;
            if (foldout)
            {
                using (new EditorGUILayout.VerticalScope())
                {
                    action.Invoke();
                }
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawMapSettings()
        {
            EditorGUILayout.PropertyField(_interactableLayer);
        }

        private void DrawInfo()
        {
            EditorGUILayout.PropertyField(_id);
            CustomEditorGUILayout.PropertyField(_title, string.IsNullOrEmpty(_title.stringValue.Trim()));
            CustomEditorGUILayout.PropertyField(_description, string.IsNullOrEmpty(_description.stringValue.Trim()));
        }

        private void DrawHotspotManagement()
        {
            DrawObjectManagementMenu<Hotspot, HotspotData>(_hotspots);
            EditorGUILayout.PropertyField(_loop);
            using (new EditorGUILayout.HorizontalScope())
            {
                _hotspotGroundOffset = EditorGUILayout.Slider(new GUIContent("Ground offset for all", "Offset all Hotspots off the ground by this amount."), _hotspotGroundOffset, 0, 100);
                if (GUILayout.Button("Overwrite", GUILayout.Width(72)))
                {
                    Undo.RegisterFullObjectHierarchyUndo(Target.gameObject, "Overwrite ground offset for all");
                    foreach (var hotspot in _hotspots)
                    {
                        if (hotspot.FindGroundPoint(100, ~0, out Vector3 ground))
                        {
                            hotspot.transform.position = ground + new Vector3(0, _hotspotGroundOffset, 0);
                            hotspot.OffsetHitbox();
                        }
                    }
                }
            }
            DrawTObjectList<Hotspot, HotspotData>(_hotspots, "Hotspots", ref _showHotspotList, ref _exportAllHotspots, ref _hotspotsGroupToggleState);
        }

        private class CustomMenuItem
        {
            public GUIContent content;
            public bool on;
            public GenericMenu.MenuFunction func;
        }

        private void DrawInteractableManagment()
        {
            DrawObjectManagementMenu<Interactable, InteractableData>(_interactables, new CustomMenuItem()
            {
                content = new GUIContent("Reset Hitbox center for all Interactables"),
                on = false,
                func = () =>
                {
                    Undo.RecordObjects(_interactables.Select(x => x.BoxCollider).ToArray(), "Reset Hitbox center for all Interactables");
                    for (int i = 0; i < _interactables.Count; i++)
                    {
                        _interactables[i].BoxCollider.center = Vector3.zero;
                    }
                }
            });
            DrawTObjectList<Interactable, InteractableData>(_interactables, "Interactables", ref _showInteractableList, ref _exportAllInteractables, ref _interactablesGroupToggleState, 3);
        }

        private void DrawObjectManagementMenu<T, D>(IList<T> objects, params CustomMenuItem[] menuItems) where T : TreasuredObject, IDataComponent<D> where D : TreasuredObjectData
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(EditorGUIUtility.TrIconContent("_Menu"), EditorStyles.label))
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent($"Regenerate Ids for {(_selectedObjectTab == 0 ? "Hotspots" : "Interactables")}"), false, () =>
                    {
                        if (EditorUtility.DisplayDialog("Warning", $"This function resets id for all {(_selectedObjectTab == 0 ? "Hotspots" : "Interactables")}. Currently target id of the Select Object action will not be update automatically and you have to manually reset it. Do you still want to proceed?", "Yes", "Cancel"))
                        {
                            foreach (var obj in objects)
                            {
                                SerializedObject o = new SerializedObject(obj);
                                SerializedProperty id = o.FindProperty("_data._id");
                                id.stringValue = Guid.NewGuid().ToString();
                                o.ApplyModifiedProperties();
                            }
                        }
                    });
                    foreach (var menuItem in menuItems)
                    {
                        menu.AddItem(menuItem.content, menuItem.on, menuItem.func);
                    }
                    menu.ShowAsContext();
                }
            }
        }

        private void DrawTObjectList<T, D>(IList<T> objects, string foldoutName, ref bool foldout, ref bool exportAll, ref GroupToggleState groupToggleState, float distance = 0) where T : TreasuredObject, IDataComponent<D> where D : TreasuredObjectData
        {
            if (objects == null)
            {
                return;
            }
            using (new EditorGUILayout.VerticalScope())
            {
                EditorGUI.indentLevel++;
                foldout = EditorGUILayout.Foldout(foldout, new GUIContent($"{foldoutName} ({objects.Count})"), true);
                if (foldout)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField(new GUIContent("Index", "The order of the Hotspot for the Guide Tour."), GUILayout.Width(64));
                        EditorGUILayout.LabelField(new GUIContent("Export", "Enable if the object should be included in the output file."), GUILayout.Width(72));
                        if (objects.Count > 1)
                        {
                            GUILayout.FlexibleSpace();
                            if (GUILayout.Button(EditorGUIUtility.TrIconContent("Transform Icon", "Edit All Transform"), EditorStyles.label, GUILayout.Width(20), GUILayout.Height(20)))
                            {
                                _currentEditingObject = null;
                                SceneView.RepaintAll();
                            }
                        }
                    }
                    if (objects.Count > 1)
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            if (objects.All(x => !x.gameObject.activeSelf))
                            {
                                exportAll = false;
                                groupToggleState = GroupToggleState.None;
                            }
                            else if (objects.Any(x => !x.gameObject.activeSelf))
                            {
                                groupToggleState = GroupToggleState.Mixed;
                            }
                            else
                            {
                                exportAll = true;
                                groupToggleState = GroupToggleState.All;
                            }
                            EditorGUI.showMixedValue = groupToggleState == GroupToggleState.Mixed;
                            GUILayout.Space(70);
                            EditorGUI.BeginChangeCheck();
                            exportAll = EditorGUILayout.ToggleLeft(GUIContent.none, exportAll);
                            if (EditorGUI.EndChangeCheck())
                            {
                                foreach (var obj in objects)
                                {
                                    obj.gameObject.SetActive(exportAll);
                                }
                            }
                            EditorGUI.showMixedValue = false;
                        }
                    }
                    for (int i = 0; i < objects.Count; i++)
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            T current = objects[i];
                            EditorGUILayout.LabelField($"{i + 1}", Styles.IndexLabel, GUILayout.Width(64));
                            EditorGUI.BeginChangeCheck();
                            bool active = EditorGUILayout.Toggle(GUIContent.none, current.gameObject.activeSelf, GUILayout.Width(20));
                            if (EditorGUI.EndChangeCheck())
                            {
                                current.gameObject.SetActive(active);
                            }
                            EditorGUILayout.LabelField(new GUIContent(current.gameObject.name, current.Data.Id));
                            if (GUILayout.Button(EditorGUIUtility.TrIconContent("Transform Icon", "Edit Transform"), EditorStyles.label, GUILayout.Width(20), GUILayout.Height(20)))
                            {
                                _currentEditingObject = current;
                                SceneView.RepaintAll();
                            }
                            using (new EditorGUI.DisabledGroupScope(!current.gameObject.activeSelf))
                            {
                                if (GUILayout.Button(EditorGUIUtility.TrIconContent("SceneViewCamera", "Move scene view to target"), EditorStyles.label, GUILayout.Width(20), GUILayout.Height(20)))
                                {
                                    current.transform.MoveSceneView(distance);
                                    _showPreview = false;
                                }
                            }
                            if (GUILayout.Button(EditorGUIUtility.TrIconContent("Search Icon", "Ping Game Object in Scene"), EditorStyles.label, GUILayout.Width(20), GUILayout.Height(20)))
                            {
                                EditorGUIUtility.PingObject(current.gameObject);
                            }
#if UNITY_2020_1_OR_NEWER // PropertyEditor only exists in 2020_1 or above https://github.com/Unity-Technologies/UnityCsReference/blob/2020.1/Editor/Mono/Inspector/PropertyEditor.cs
                            if (GUILayout.Button(EditorGUIUtility.TrIconContent("UnityEditor.InspectorWindow", "Open Editor Window"), EditorStyles.label, GUILayout.Width(20), GUILayout.Height(20)))
                            {
                                ReflectionUtility.OpenPropertyEditor(current);
                            }
#endif
                        }
                    }
                }
                EditorGUI.indentLevel--;
                EditorGUILayout.Space(16);
                if (GUILayout.Button(new GUIContent($"Create new {(_selectedObjectTab == 0 ? "Hotspot" : "Interactable")}")))
                {
                    if (_selectedObjectTab == 0)
                    {
                        Transform root = GetChild(Target.transform, "Hotspots");
                        Hotspot hotspot = CreateTreasuredObject<Hotspot>(root);
                        hotspot.gameObject.name = $"Hotspot {_hotspots.Count + 1}";
                        _hotspots.Add(hotspot);
                        if(_currentEditingObject != null)
                        {
                            hotspot.transform.position = _currentEditingObject.gameObject.transform.position;
                            _currentEditingObject = hotspot;
                        }
                    }
                    else
                    {
                        Transform root = GetChild(Target.transform, "Interactables");
                        Interactable interactable = CreateTreasuredObject<Interactable>(root);
                        interactable.gameObject.name = $"Interactable {_interactables.Count + 1}";
                        _interactables.Add(interactable);
                        if (_currentEditingObject != null)
                        {
                            interactable.transform.position = _currentEditingObject.gameObject.transform.position;
                            _currentEditingObject = interactable;
                        }
                    }
                }
            }
        }

        private void DrawExportSettings()
        {
            EditorGUILayout.PropertyField(_format);
            EditorGUILayout.PropertyField(_quality);
            CustomEditorGUILayout.FolderField(_outputDirectory, new GUIContent("Output Directory", "The root folder for the outputs which is relative to the Project path."), $"/{_sceneName}");
            _showInExplorer = EditorGUILayout.Toggle(new GUIContent("Show In Explorer", "Opens the output directory once the exporting is done if enabled."), _showInExplorer);
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledGroupScope(_hotspots.Count == 0))
                {
                    if (GUILayout.Button("Export Panoramic Images", GUILayout.Height(24)))
                    {
                        string outputDirectory = GetAbosluteOutputDirectory(_sceneName);
                        Directory.CreateDirectory(outputDirectory);
                        ExportPanoramicImages(outputDirectory);
                        if (_showInExplorer)
                        {
                            Application.OpenURL(outputDirectory);
                        }
                    }
                }
                using (new EditorGUI.DisabledGroupScope(!IsAllRequiredFieldFilled()))
                {
                    if (GUILayout.Button("Export Json", GUILayout.Height(24)))
                    {
                        string outputDirectory = GetAbosluteOutputDirectory(_sceneName);
                        Directory.CreateDirectory(outputDirectory);
                        ExportJson(outputDirectory);
                        if (_showInExplorer)
                        {
                            Application.OpenURL(outputDirectory);
                        }
                    }
                    if (GUILayout.Button("Export All", GUILayout.Height(24)))
                    {
                        string outputDirectory = GetAbosluteOutputDirectory(_sceneName);
                        Directory.CreateDirectory(outputDirectory);
                        ExportAll(outputDirectory);
                        if (_showInExplorer)
                        {
                            Application.OpenURL(outputDirectory);
                        }
                    }
                }
            }
        }

        private bool IsAllRequiredFieldFilled()
        {
            bool allFilled = !string.IsNullOrEmpty(_title.stringValue.Trim());
            allFilled &= !string.IsNullOrEmpty(_description.stringValue.Trim());
            allFilled &= !string.IsNullOrEmpty(_outputDirectory.stringValue);
            return allFilled;
        }
    }
}
