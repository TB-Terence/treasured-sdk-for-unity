using System;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using System.Linq;
using Treasured.UnitySdk.Utilities;
using System.Collections.Generic;
using System.Text;

namespace Treasured.UnitySdk
{
    internal class GuidedTourEditorWindow : EditorWindow
    {
        class GuidedTourSettingsWindow: EditorWindow
        {
            public static float sleepDuration = 3;

            private void OnGUI()
            {
                sleepDuration = EditorGUILayout.FloatField("Default Sleep Duration", sleepDuration);
            }
        }
        public static GuidedTourEditorWindow ShowWindow(TreasuredScene scene)
        {
            var window = EditorWindow.GetWindow<GuidedTourEditorWindow>();
            window.scene = scene;
            window.titleContent = new GUIContent("Guided Tour Editor");
            window.minSize = new Vector2(600, 400);
            window.Initialize();
            window.Show();
            return window;
        }

        private static GuidedTourGraph _current;
        public static GuidedTourGraph Current
        {
            get
            {
                return _current;
            }
        }

        TreasuredScene scene;
        Vector2 tourListScrollPosition;
        ReorderableList tourList;
        Vector2 actionListScrollPosition;
        ReorderableList actionList;
        Vector2 actionInfoScrollPosition;

        SerializedObject serializedObject;
        SerializedObject serializedTour;

        SerializedProperty selectedAction;

        GuidedTourSettingsWindow settingsWindow;

        private void OnEnable()
        {
            scene ??= Selection.activeGameObject.GetComponent<TreasuredScene>();
            if (scene) Initialize();
        }

        void Initialize()
        {
            serializedObject = new SerializedObject(scene);
            if (tourList == null)
            {
                tourList = new ReorderableList(scene.graph.tours, typeof(GuidedTour));
            }
            _current = scene.graph;
            tourList.index = 0;
            tourList.onSelectCallback += OnTourChanged;
            tourList.draggable = false;
            tourList.displayAdd = tourList.displayRemove = false;
            tourList.headerHeight = 0;
            tourList.drawNoneElementCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, new GUIContent("No tour available"));
            };
            tourList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                GuidedTour tour = (GuidedTour)tourList.list[index];
                EditorGUI.LabelField(rect, tour.isDefault ?  EditorGUIUtility.TrTextContentWithIcon(tour.title, "d_NavMeshData Icon") : new GUIContent(tour.title));
            };
            tourList.onMouseUpCallback = (ReorderableList list) =>
            {
                if (Event.current.button == 1)
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Set as Default"), false, () =>
                    {
                        GuidedTour selectedTour = tourList.list[tourList.index] as GuidedTour;
                        selectedTour.isDefault = true;
                        foreach (GuidedTour tour in tourList.list)
                        {
                            if (tour == selectedTour) continue;
                            tour.isDefault = false;
                        }
                    });
                    menu.ShowAsContext();
                }
            };
            OnTourChanged(tourList);
        }

        private GuidedTour CreateNew(ReorderableList list, GuidedTourGraph graph, string defaultName = "untitled")
        {
            var tour = GuidedTour.CreateInstance<GuidedTour>();
            graph.tours.Add(tour);
            if (graph.tours.Count == 1)
            {
                tour.name = tour.title = "untitled";
                tour.isDefault = true;
            }
            else
            {
                string[] tourNames = scene.graph?.tours.Select(tour => tour.name).ToArray();
                tour.title = tour.name = ObjectNames.GetUniqueName(tourNames, defaultName);
            }
            list.index = list.count - 1;
            OnTourChanged(list);
            this.Repaint();
            return tour as GuidedTour;
        }

        void OnTourChanged(ReorderableList tourList)
        {
            if (tourList.index < 0 || tourList.index >= scene.graph.tours.Count)
            {
                return;
            }
            serializedTour = new SerializedObject(scene.graph.tours[tourList.index]);
            selectedAction = null;
            actionList = new ReorderableList(scene.graph.tours[tourList.index].actions, typeof(ScriptableActionCollection));
            actionList.headerHeight = 0;
            actionList.displayAdd = actionList.displayRemove = false;
            actionList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                serializedTour.Update();
                ScriptableAction action = (ScriptableAction)scene.graph.tours[tourList.index].actions[index];
                GUIContent label = new GUIContent(ObjectNames.NicifyVariableName(action.Type));
                float width = EditorGUIUtility.labelWidth;
                Vector2 size = EditorStyles.label.CalcSize(label);
                EditorGUIUtility.labelWidth = size.x;
                switch (action)
                {
                    case GoToAction goToAction:
                        goToAction.target = (Hotspot)EditorGUI.ObjectField(rect, label, goToAction.target, typeof(Hotspot), true);
                        break;
                    case SleepAction sleepAction:
                        sleepAction.duration = EditorGUI.FloatField(rect, label, sleepAction.duration);
                        break;
                    case PanAction panAction:
                        panAction.amount = EditorGUI.FloatField(rect, label, panAction.amount);
                        break;
                    default:
                        EditorGUI.LabelField(rect, new GUIContent(label));
                        break;
                }
                EditorGUIUtility.labelWidth = width;
                serializedTour.ApplyModifiedProperties();
            };
            actionList.onSelectCallback += (ReorderableList list) =>
            {
                SerializedProperty actionCollection = serializedTour.FindProperty("actions");
                selectedAction = actionCollection.FindPropertyRelative("_actions").GetArrayElementAtIndex(list.index);
            };
        }

        private string GetNicifyActionName(Type type)
        {
            string name = type.Name;
            if (name.EndsWith("Action") && name.Length > 6)
            {
                name = name.Substring(0, name.Length - 6);
            }
            return ObjectNames.NicifyVariableName(name);
        }

        private void OnGUI()
        {
            serializedObject.Update();
            using (new GUILayout.HorizontalScope())
            {
                using (new GUILayout.VerticalScope(GUILayout.Width(200)))
                {
                    GUILayout.Space(2);
                    using(new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label(new GUIContent("Tours"), EditorStyles.boldLabel);
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("Settings")), EditorStyles.label, GUILayout.Width(18))) 
                        {
                            settingsWindow = EditorWindow.GetWindow<GuidedTourSettingsWindow>(true, "Guided Tour Settings", true);
                            settingsWindow.position = new Rect(0, 0, 400, 200) { center = position.center };
                            settingsWindow.Show();
                        }
                        if (GUILayout.Button(EditorGUIUtility.TrIconContent("d_Toolbar Plus More"), EditorStyles.label, GUILayout.Width(18)))
                        {
                            GenericMenu menu = new GenericMenu();
                            menu.AddItem(new GUIContent("New"), false, () =>
                            {
                                CreateNew(tourList, scene.graph);
                                serializedTour.Update();
                            });
                            menu.AddItem(new GUIContent("Quick Tour/Navigate Through Hotspots"), false, () =>
                            {
                                GuidedTour tour = CreateNew(tourList, scene.graph, "Quick Tour");
                                foreach (var hotspot in scene.Hotspots)
                                {
                                    if (!hotspot.gameObject.activeInHierarchy) continue;
                                    tour.actions.Add(new GoToAction()
                                    {
                                        target = hotspot
                                    });
                                    tour.actions.Add(new SetCameraRotationAction()
                                    {
                                        rotation = hotspot.Camera.transform.rotation
                                    });
                                    tour.actions.Add(new SleepAction()
                                    {
                                        duration = GuidedTourSettingsWindow.sleepDuration
                                    });
                                }
                                serializedTour.Update();
                                serializedTour.ApplyModifiedProperties();
                            });
                            menu.AddItem(new GUIContent("Quick Tour/Navigate Through Hotspots with On Select"), false, () =>
                            {
                                GuidedTour tour = CreateNew(tourList, scene.graph, "Quick Tour");
                                foreach (var hotspot in scene.Hotspots)
                                {
                                    if (!hotspot.gameObject.activeInHierarchy) continue;
                                    tour.actions.Add(new GoToAction()
                                    {
                                        target = hotspot
                                    });
                                    tour.actions.Add(new SetCameraRotationAction()
                                    {
                                        rotation = hotspot.Camera.transform.rotation
                                    });
                                    if (hotspot.actionGraph.TryGetActionGroup("onSelect", out var onSelect))
                                    {
                                        foreach (ScriptableAction action in onSelect)
                                        {
                                            if(action.enabled)
                                                tour.actions.Add(action);
                                        }
                                    }
                                    tour.actions.Add(new SleepAction()
                                    {
                                        duration = GuidedTourSettingsWindow.sleepDuration
                                    });
                                }
                            });
                            menu.ShowAsContext();
                        }
                        if (GUILayout.Button(EditorGUIUtility.TrIconContent("d_Toolbar Minus"), EditorStyles.label, GUILayout.Width(18)))
                        {
                            if(tourList.index > -1 && tourList.index < tourList.count)
                            {
                                bool wasDefault = (tourList.list[tourList.index] as GuidedTour).isDefault;
                                tourList.list.RemoveAt(tourList.index);
                                serializedTour = null;
                                if (tourList.index >= tourList.count)
                                {
                                    tourList.index = tourList.count - 1;
                                }
                                if (wasDefault && tourList.list.Count > 0)
                                {
                                    (tourList.list[tourList.index] as GuidedTour).isDefault = true;
                                }
                            }
                        }
                    }
                    using (var tourScope = new GUILayout.ScrollViewScope(tourListScrollPosition, GUILayout.MinHeight(120)))
                    {
                        tourListScrollPosition = tourScope.scrollPosition;
                        tourList.DoLayoutList();
                    }
                    GUILayout.Space(2);
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label(new GUIContent("Actions"), EditorStyles.boldLabel);
                        using (new EditorGUI.DisabledGroupScope(serializedTour == null))
                        {
                            if (GUILayout.Button(EditorGUIUtility.TrIconContent("d_RotateTool", "Use default value"), EditorStyles.label, GUILayout.Width(18)))
                            {
                                foreach (var action in scene.graph.tours[tourList.index].actions)
                                {
                                    switch (action)
                                    {
                                        case SleepAction sleepAction:
                                            sleepAction.duration = GuidedTourSettingsWindow.sleepDuration;
                                            break;
                                        default:
                                            break;
                                    }
                                }
                            }
                            if (GUILayout.Button(EditorGUIUtility.TrIconContent("d_Toolbar Plus More"), EditorStyles.label, GUILayout.Width(18)))
                            {
                                var actionTypes = UnityEditor.TypeCache.GetTypesDerivedFrom<ScriptableAction>().Where(x => x.IsDefined(typeof(APIAttribute), true) && !x.IsAbstract && !x.IsDefined(typeof(ObsoleteAttribute), true));
                                GenericMenu menu = new GenericMenu();
                                foreach (var type in actionTypes)
                                {
                                    // TODO: Fix tour picker drawer
                                    if (type.Equals(typeof(StartTourAction))) continue;
                                    CategoryAttribute attribute = (CategoryAttribute)Attribute.GetCustomAttribute(type, typeof(CategoryAttribute));
                                    string nicfyName = GetNicifyActionName(type);
                                    menu.AddItem(new GUIContent(attribute != null ? $"{attribute.Path}/{nicfyName}" : nicfyName), false, () =>
                                    {
                                        int index = actionList.index == -1 ? 0 : actionList.index;
                                        SerializedProperty collection = serializedTour.FindProperty("actions").FindPropertyRelative("_actions");
                                        collection.InsertManagedObject(type, index);
                                        serializedTour.Update();
                                        serializedTour.ApplyModifiedProperties();
                                        // TODO: Update scroll view
                                        actionList.index = index + 1;
                                        selectedAction = collection.GetArrayElementAtIndex(index);
                                        actionList?.onSelectCallback?.Invoke(actionList);
                                    });
                                    CreateActionGroupAttribute[] linkedActionAttributes = (CreateActionGroupAttribute[])Attribute.GetCustomAttributes(type, typeof(CreateActionGroupAttribute));
                                    if (linkedActionAttributes != null && linkedActionAttributes.Length > 0)
                                    {
                                        foreach (var linkedActionAttribute in linkedActionAttributes)
                                        {
                                            StringBuilder pathBuilder = new StringBuilder();
                                            pathBuilder.Append(attribute != null ? $"{attribute.Path}/{nicfyName}(Group)" : $"{nicfyName}(Group)/");
                                            for (int i = 0; i < linkedActionAttribute.Types.Length; i++)
                                            {
                                                if (i > 0)
                                                {
                                                    pathBuilder.Append(", ");
                                                }
                                                pathBuilder.Append(GetNicifyActionName(linkedActionAttribute.Types[i]));
                                            }
                                            menu.AddItem(new GUIContent(pathBuilder.ToString()), false, () =>
                                            {
                                                int index = actionList.index == -1 ? -1 : actionList.index;
                                                SerializedProperty collection = serializedTour.FindProperty("actions").FindPropertyRelative("_actions");
                                                SerializedProperty element = collection.InsertManagedObject(type, index++);
                                                foreach (var linkedType in linkedActionAttribute.Types)
                                                {
                                                    collection.InsertManagedObject(linkedType, index++);
                                                }
                                                element.isExpanded = element.managedReferenceFullTypename.EndsWith("GroupAction") ? false : true;
                                                element.serializedObject.ApplyModifiedProperties();
                                                actionList.index = index > actionList.count - 1 ? actionList.count - 1 : index;
                                                actionList?.onSelectCallback?.Invoke(actionList);
                                            });
                                        }
                                    }
                                }
                                // TODO: Implement insert actions from selected object's action graph
                                if (Selection.activeGameObject.TryGetComponent(out TreasuredObject obj))
                                {
                                    menu.AddSeparator("");
                                    SerializedProperty collection = serializedTour.FindProperty("actions").FindPropertyRelative("_actions");
                                    SerializedObject tour = new SerializedObject(obj);
                                    SerializedProperty graph = tour.FindProperty(nameof(TreasuredObject.actionGraph));
                                    foreach (var actionGroup in obj.actionGraph.GetGroups())
                                    {
                                        menu.AddItem(new GUIContent($"Insert ({obj.name})/{ObjectNames.NicifyVariableName(actionGroup.name)}"), false, () =>
                                        {
                                            ActionCollection list = (ActionCollection)actionList.list;
                                            int index = actionList.index < 1 ? 0 : actionList.index;
                                            for (int i = 0; i < actionGroup.Count; i++)
                                            {
                                                Type type = actionGroup[i].GetType();
                                                ScriptableAction action = (ScriptableAction)Activator.CreateInstance(type);
                                                list.Insert(i + index, action);
                                                EditorUtility.CopySerializedManagedFieldsOnly(actionGroup[i], action);
                                            }
                                            serializedObject.Update();
                                            serializedObject.ApplyModifiedProperties();
                                        });
                                    }
                                }
                                else
                                {
                                    menu.AddSeparator("");
                                    menu.AddDisabledItem(new GUIContent("Insert (No selection)"));
                                }
                                menu.ShowAsContext();
                            }
                        }
                        if (GUILayout.Button(EditorGUIUtility.TrIconContent("d_Toolbar Minus"), EditorStyles.label, GUILayout.Width(18)))
                        {
                            if (actionList.index > -1 && actionList.index < actionList.count)
                            {
                                actionList.list.RemoveAt(actionList.index);
                                selectedAction = null;
                                if (actionList.index >= actionList.count)
                                {
                                    actionList.index = actionList.count - 1;
                                }
                            }
                        }
                    }
                    using (var actionScope = new GUILayout.ScrollViewScope(actionListScrollPosition, GUILayout.ExpandHeight(true)))
                    {
                        actionListScrollPosition = actionScope.scrollPosition;
                        if (serializedTour != null)
                        {
                            actionList?.DoLayoutList();
                        }
                    }
                }
                using (new GUILayout.VerticalScope())
                {
                    GUILayout.Label("Guided Tour Info", EditorStyles.whiteLargeLabel);
                    EditorGUI.indentLevel++;
                    if (serializedTour != null)
                    {
                        EditorGUIUtils.DrawPropertiesExcluding(serializedTour, "m_Script", "actions");
                    }
                    else
                    {
                        EditorGUILayout.LabelField("No Tour is selected", EditorStyles.centeredGreyMiniLabel);
                    }
                    EditorGUI.indentLevel--;
                    GUILayout.Label("Action Info", EditorStyles.whiteLargeLabel);
                    EditorGUI.indentLevel++;
                    using (var actionInfoScope = new GUILayout.ScrollViewScope(actionInfoScrollPosition, GUILayout.ExpandHeight(true)))
                    {
                        actionInfoScrollPosition = actionInfoScope.scrollPosition;
                        if (selectedAction != null)
                        {
                            selectedAction.serializedObject.Update();
                            EditorGUIUtils.DrawPropertyWithoutFoldout(selectedAction);
                            selectedAction.serializedObject.ApplyModifiedProperties();
                        }
                        else
                        {
                            EditorGUILayout.LabelField("No action is selected", EditorStyles.centeredGreyMiniLabel);
                        }
                    }
                    EditorGUI.indentLevel--;
                }
            }
                    serializedObject.ApplyModifiedProperties();
        }
    }
}
