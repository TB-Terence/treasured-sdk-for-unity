using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Treasured.UnitySdk.Editor
{
    [CustomEditor(typeof(TreasuredMap))]
    internal sealed partial class TreasuredMapEditor : TreasuredEditor<TreasuredMap>
    {
        static class GUIText
        {
            public static readonly GUIContent alignView = EditorGUIUtility.TrTextContent("Align View");
            public static readonly GUIContent snapAllToGround = EditorGUIUtility.TrTextContent("Snap All on Ground");
            public static readonly GUIContent selectAll = EditorGUIUtility.TrTextContent("Select All");
        }

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

        #region Version 0.5
        private TreasuredMapExporter exporter;

        private SerializedProperty title;
        private SerializedProperty description;

        private SerializedProperty format;
        private SerializedProperty quality;

        private SerializedProperty loop;

        private bool exportAllHotspots = true;
        private GroupToggleState hotspotsGroupToggleState = GroupToggleState.All;
        private Vector2 hotspotsScrollPosition;

        private bool exportAllInteractables = true;
        private GroupToggleState interactablesGroupToggleState = GroupToggleState.All;
        private Vector2 interactablesScrollPosition;

        private int selectedObjectListIndex = 0;
        private static readonly string[] selectableObjectListNames = new string[] { "Hotspots", "Interactables" };

        private Dictionary<string, bool> showGroupFlags = new Dictionary<string, bool>();

        //private TreasuredMapExporter exporter;

        private List<Hotspot> hotspots = new List<Hotspot>();
        private List<Interactable> interactables = new List<Interactable>();
        #endregion

        protected override void Init()
        {
            //TreasuredMapEditorUtility.RefreshIds(Target);
            //Target.transform.hideFlags = HideFlags.HideInInspector;
            //_hotspots = Target.gameObject.GetComponentsInChildren<Hotspot>(true).ToList();
            //_interactables = Target.gameObject.GetComponentsInChildren<Interactable>(true).ToList();
            //Target.Data.GenerateHotspots(_hotspots);
            //Target.Data.GenerateInteractables(_interactables);
            //Target.Data.Validate();
            InitSerializedProperty();
            //Tools.hidden = true;

            hotspots = Target.gameObject.GetComponentsInChildren<Hotspot>(true).ToList();
            interactables = Target.gameObject.GetComponentsInChildren<Interactable>(true).ToList();

            exporter = new TreasuredMapExporter(serializedObject, Target);

            SceneView.duringSceneGui -= OnSceneViewGUI;
            SceneView.duringSceneGui += OnSceneViewGUI;
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
            for (int i = 0; i < hotspots.Count; i++)
            {
                Hotspot current = hotspots[i];
                Hotspot next = hotspots[(i + 1) % hotspots.Count];
                Handles.color = Color.white;
                Handles.Label(current.cameraTransform.Position, current.name);

                if (!loop.boolValue && i == hotspots.Count - 1)
                {
                    continue;
                }
                Handles.DrawLine(current.cameraTransform.Position, next.cameraTransform.Position);
                Vector3 direction = next.cameraTransform.Position - current.cameraTransform.Position;
                if (direction != Vector3.zero)
                {
                    Handles.color = Color.green;
                    Handles.ArrowHandleCap(0, current.cameraTransform.Position, Quaternion.LookRotation(direction), 0.5f, EventType.Repaint);
                }
            }
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

            title = serializedObject.FindProperty(nameof(title));
            description = serializedObject.FindProperty(nameof(description));
            loop = serializedObject.FindProperty(nameof(loop));
        }


        public override void OnInspectorGUI()
        {
            Styles.Init();
            serializedObject.Update();
            //using (new EditorGUILayout.VerticalScope())
            //{
            //    OnFoldoutGroupGUI(ref _showMapSettings, new GUIContent("Map Settings"), OnDrawMapSettings);
            //    OnFoldoutGroupGUI(ref _showInfo, new GUIContent("Info"), OnDrawInfo);
            //    OnFoldoutGroupGUI(ref _showManagementTabs, new GUIContent("Object Management"), OnDrawObjectManagement);
            //    //OnFoldoutGroupGUI(ref _showExportSettings, new GUIContent("Export Settings"), OnDrawExportSettings);
            //    OnFooter();
            //}
            if (GUILayout.Button("Upgrade to Version 0.5.0", GUILayout.Height(36)))
            {
                if (Target != null && Target.Data != null)
                {
                    serializedObject.FindProperty("_id").stringValue = Target.Data.Id;
                    title.stringValue = Target.Data.Title;
                    description.stringValue = Target.Data.Description;
                    loop.boolValue = Target.Data.Loop;
                    foreach (var hotspot in hotspots)
                    {
                        SerializedObject obj = new SerializedObject(hotspot);
                        SerializedProperty data = obj.FindProperty("_data");
                        SerializedProperty oldId = data.FindPropertyRelative("_id");
                        SerializedProperty newId = obj.FindProperty("_id");
                        newId.stringValue = oldId.stringValue;

                        obj.ApplyModifiedProperties();
                    }
                    foreach (var interactable in interactables)
                    {
                        SerializedObject obj = new SerializedObject(interactable);
                        SerializedProperty data = obj.FindProperty("_data");
                        SerializedProperty oldId = data.FindPropertyRelative("_id");
                        SerializedProperty newId = obj.FindProperty("_id");
                        newId.stringValue = oldId.stringValue;

                        obj.ApplyModifiedProperties();
                    }
                }
            }
            OnLaunchPageSettingsGUI();
            OnGuideTourSettingsGUI();
            OnObjectManagementGUI();
            OnExportGUI();
            serializedObject.ApplyModifiedProperties();
        }

        void OnLaunchPageSettingsGUI()
        {
            OnFoldoutGroupGUI("Launch Page Settings", () =>
            {
                EditorGUILayout.PropertyField(title);
                EditorGUILayout.PropertyField(description);
                //EditorGUILayout.PropertyField(cover);
                //if (cover.objectReferenceValue is Texture2D preview)
                //{
                //    Rect previewRect = EditorGUILayout.GetControlRect(false, height: 128);
                //    EditorGUI.DrawPreviewTexture(previewRect, preview, null, ScaleMode.ScaleToFit);
                //}
            });
        }

        void OnGuideTourSettingsGUI()
        {
            OnFoldoutGroupGUI("Guide Tour Settings", () =>
            {
                EditorGUILayout.PropertyField(loop);
            });
        }

        void OnObjectManagementGUI()
        {
            OnFoldoutGroupGUI("Object Management", () =>
            {
                EditorGUI.indentLevel++;
                selectedObjectListIndex = GUILayout.SelectionGrid(selectedObjectListIndex, selectableObjectListNames, selectableObjectListNames.Length);
                if (selectedObjectListIndex == 0)
                {
                    OnObjectList(hotspots, ref hotspotsScrollPosition, ref exportAllHotspots, ref hotspotsGroupToggleState);
                }
                else if (selectedObjectListIndex == 1)
                {
                    OnObjectList(interactables, ref interactablesScrollPosition, ref exportAllInteractables, ref interactablesGroupToggleState);
                }
                EditorGUI.indentLevel--;
            }, true);
        }

        void OnObjectList<T>(IList<T> objects, ref Vector2 scrollPosition, ref bool exportAll, ref GroupToggleState groupToggleState) where T : TreasuredObject
        {
            using (new EditorGUILayout.VerticalScope(style: "box"))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(new GUIContent("Index", "The order of the Hotspot for the Guide Tour."), GUILayout.Width(64));
                    EditorGUILayout.LabelField(new GUIContent("Export", "Enable if the object should be included in the output file."), GUILayout.Width(72));
                    //GUILayout.FlexibleSpace();
                    //if (GUILayout.Button(Icons.menu, EditorStyles.label, GUILayout.Width(20), GUILayout.Height(20)))
                    //{
                    //    ShowObjectsMenu(objects);
                    //};
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
                using (var scope = new EditorGUILayout.ScrollViewScope(scrollPosition, GUILayout.Height(220)))
                {
                    scrollPosition = scope.scrollPosition;
                    for (int i = 0; i < objects.Count; i++)
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            T current = objects[i];
                            EditorGUILayout.LabelField($"{i + 1}", GUILayout.Width(64));
                            EditorGUI.BeginChangeCheck();
                            bool active = EditorGUILayout.Toggle(GUIContent.none, current.gameObject.activeSelf, GUILayout.Width(20));
                            if (EditorGUI.EndChangeCheck())
                            {
                                current.gameObject.SetActive(active);
                            }
                            EditorGUILayout.LabelField(new GUIContent(current.gameObject.name, current.Id));
                            if (EditorGUILayoutUtilities.CreateClickZone(Event.current, GUILayoutUtility.GetLastRect(), MouseCursor.Link, 0))
                            {
                                Selection.activeGameObject = current.gameObject;
                            }
                            using (new EditorGUI.DisabledGroupScope(!current.gameObject.activeSelf))
                            {
                                if (GUILayout.Button(Icons.menu, EditorStyles.label, GUILayout.Width(20), GUILayout.Height(20)))
                                {
                                    ShowObjectMenu(current);
                                };
                            }
                        }
                    }
                }
            }
        }

        void ShowObjectMenu(TreasuredObject obj)
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(GUIText.alignView, false, () =>
            {
                if (obj is Hotspot hotspot)
                {
                    SceneView.lastActiveSceneView.LookAt(hotspot.transform.position + hotspot.cameraPositionOffset, hotspot.transform.rotation, 0.01f);
                }
                else
                {
                    Vector3 targetPosition = obj.transform.position;
                    Vector3 cameraPosition = obj.transform.position + obj.transform.forward * 5f;
                    SceneView.lastActiveSceneView.LookAt(cameraPosition, Quaternion.LookRotation(targetPosition - cameraPosition), 5f);
                }
            });
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Ping"), false, () =>
            {
                EditorGUIUtility.PingObject(obj.gameObject);
            });
#if UNITY_2020_1_OR_NEWER // PropertyEditor only exists in 2020_1 or above https://github.com/Unity-Technologies/UnityCsReference/blob/2020.1/Editor/Mono/Inspector/PropertyEditor.cs
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Edit"), false, () =>
            {
                ReflectionUtilities.OpenPropertyEditor(obj);
            });
#endif
            menu.ShowAsContext();
        }

        void ShowObjectsMenu<T>(IList<T> objects) where T : TreasuredObject
        {
            GenericMenu menu = new GenericMenu();
            if (typeof(T) == typeof(Hotspot))
            {
                menu.AddItem(GUIText.snapAllToGround, false, () =>
                {
                    foreach (var hotspot in objects as IList<Hotspot>)
                    {
                        hotspot.SnapToGround();
                    }
                });
                menu.AddSeparator("");
            }
            menu.AddItem(GUIText.selectAll, false, () =>
            {
                Selection.objects = objects.Select(x => x.gameObject).ToArray();
            });
            menu.ShowAsContext();
        }

        void OnExportGUI()
        {
            OnFoldoutGroupGUI("Export", () =>
            {
                exporter?.OnGUI();
            }, false);
        }

        void OnFoldoutGroupGUI(string groupName, Action action, bool isExpandByDefault = false)
        {
            if (!showGroupFlags.ContainsKey(groupName))
            {
                showGroupFlags[groupName] = isExpandByDefault;
            }
            showGroupFlags[groupName] = EditorGUILayout.BeginFoldoutHeaderGroup(showGroupFlags[groupName], new GUIContent(groupName));
            if (showGroupFlags[groupName])
            {
                action.Invoke();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void OnFooter()
        {
            if (GUILayout.Button("Open Upload URL", GUILayout.Height(24f)))
            {
                Application.OpenURL("https://dev.world.treasured.ca/upload");
            }
        }

        private void OnDrawObjectManagement()
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

        private void OnFoldoutGroupGUI(ref bool foldout, GUIContent label, Action action)
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

        private void OnDrawMapSettings()
        {
            EditorGUILayout.PropertyField(_interactableLayer);
        }

        private void OnDrawInfo()
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
                                ReflectionUtilities.OpenPropertyEditor(current);
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

        //private void OnDrawExportSettings()
        //{
        //    EditorGUILayout.PropertyField(_format);
        //    EditorGUILayout.PropertyField(_quality);
        //    CustomEditorGUILayout.FolderField(_outputDirectory, new GUIContent("Output Directory", "The root folder for the outputs which is relative to the Project path."), $"/{_sceneName}");
        //    _showInExplorer = EditorGUILayout.Toggle(new GUIContent("Show In Explorer", "Opens the output directory once the exporting is done if enabled."), _showInExplorer);
        //    using (new EditorGUILayout.HorizontalScope())
        //    {
        //        using (new EditorGUI.DisabledGroupScope(_hotspots.Count == 0))
        //        {
        //            if (GUILayout.Button("Export Panoramic Images", GUILayout.Height(24)))
        //            {
        //                string outputDirectory = GetAbosluteOutputDirectory(_sceneName);
        //                Directory.CreateDirectory(outputDirectory);
        //                ExportPanoramicImages(outputDirectory);
        //                if (_showInExplorer)
        //                {
        //                    Application.OpenURL(outputDirectory);
        //                }
        //            }
        //        }
        //        using (new EditorGUI.DisabledGroupScope(!IsAllRequiredFieldFilled()))
        //        {
        //            if (GUILayout.Button("Export Json", GUILayout.Height(24)))
        //            {
        //                string outputDirectory = GetAbosluteOutputDirectory(_sceneName);
        //                Directory.CreateDirectory(outputDirectory);
        //                ExportJson(outputDirectory);
        //                if (_showInExplorer)
        //                {
        //                    Application.OpenURL(outputDirectory);
        //                }
        //            }
        //            if (GUILayout.Button("Export All", GUILayout.Height(24)))
        //            {
        //                string outputDirectory = GetAbosluteOutputDirectory(_sceneName);
        //                Directory.CreateDirectory(outputDirectory);
        //                ExportAll(outputDirectory);
        //                if (_showInExplorer)
        //                {
        //                    Application.OpenURL(outputDirectory);
        //                }
        //            }
        //        }
        //    }
        //}

        private bool IsAllRequiredFieldFilled()
        {
            bool allFilled = !string.IsNullOrEmpty(_title.stringValue.Trim());
            allFilled &= !string.IsNullOrEmpty(_description.stringValue.Trim());
            allFilled &= !string.IsNullOrEmpty(_outputDirectory.stringValue);
            return allFilled;
        }
    }
}
