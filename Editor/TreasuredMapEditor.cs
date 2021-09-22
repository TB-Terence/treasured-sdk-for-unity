using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk.Editor
{
    [CustomEditor(typeof(TreasuredMap))]
    internal sealed partial class TreasuredMapEditor : TreasuredEditor<TreasuredMap>
    {
        private static readonly string[] selectableObjectListNames = new string[] { "Hotspots", "Interactables" };

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

        private SerializedProperty _interactableLayer;

        private TreasuredObject _currentEditingObject = null;
        private int _selectedObjectTab;

        #region Hotspot Management
        private float _hotspotGroundOffset = 2;
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

        protected override void Init()
        {
            GetFoldoutGroupMethods();

            _interactableLayer = serializedObject.FindProperty(nameof(_interactableLayer));

            _title = serializedObject.FindProperty(nameof(_title));
            _description = serializedObject.FindProperty(nameof(_description));

            _loop = serializedObject.FindProperty(nameof(_loop));

            _format = serializedObject.FindProperty(nameof(_format));
            _quality = serializedObject.FindProperty(nameof(_quality));

            hotspots = Target.gameObject.GetComponentsInChildren<Hotspot>(true).ToList();
            interactables = Target.gameObject.GetComponentsInChildren<Interactable>(true).ToList();

            exporter = new TreasuredMapExporter(serializedObject, Target);

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
            for (int i = 0; i < hotspots.Count; i++)
            {
                Hotspot current = hotspots[i];
                Hotspot next = hotspots[(i + 1) % hotspots.Count];

                Vector3 currentCameraPosition = current.transform.position + current.CameraPositionOffset;
                Vector3 nextCameraPosition = next.transform.position + next.CameraPositionOffset;

                Handles.color = Color.white;
                Handles.Label(currentCameraPosition, current.name);

                if (!_loop.boolValue && i == hotspots.Count - 1)
                {
                    continue;
                }
                Handles.DrawLine(currentCameraPosition, nextCameraPosition);
                Vector3 direction = nextCameraPosition - currentCameraPosition;
                if (direction != Vector3.zero)
                {
                    Handles.color = Color.green;
                    Handles.ArrowHandleCap(0, currentCameraPosition, Quaternion.LookRotation(direction), 0.5f, EventType.Repaint);
                }
            }
        }

        public override void OnInspectorGUI()
        {
            Styles.Init();
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

        [FoldoutGroup("Object Management")]
        
        void OnObjectManagementGUI()
        {
            selectedObjectListIndex = GUILayout.SelectionGrid(selectedObjectListIndex, selectableObjectListNames, selectableObjectListNames.Length);
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
            using (new EditorGUILayout.VerticalScope(style: "box"))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(new GUIContent("Index", "The order of the Hotspot for the Guide Tour."), GUILayout.Width(64));
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
                using (var scope = new EditorGUILayout.ScrollViewScope(scrollPosition, GUILayout.Height(220)))
                {
                    scrollPosition = scope.scrollPosition;
                    for (int i = 0; i < objects.Count; i++)
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            T current = objects[i];
                            EditorGUILayout.LabelField($"{i + 1}", GUILayout.Width(64));
                            //EditorGUI.BeginChangeCheck();
                            //bool active = EditorGUILayout.Toggle(GUIContent.none, current.gameObject.activeSelf, GUILayout.Width(20));
                            //if (EditorGUI.EndChangeCheck())
                            //{
                            //    current.gameObject.SetActive(active);
                            //}
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
                    SceneView.lastActiveSceneView.LookAt(hotspot.transform.position + hotspot.CameraPositionOffset, hotspot.transform.rotation, 0.01f);
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
    }
}
