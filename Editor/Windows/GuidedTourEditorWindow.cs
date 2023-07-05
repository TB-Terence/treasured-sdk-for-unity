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

        TreasuredScene scene;
        Vector2 tourListScrollPosition;
        ReorderableList tourList;
        Vector2 actionListScrollPosition;
        ReorderableList actionList;

        SerializedObject serializedObject;
        SerializedObject serializedTour;

        SerializedProperty selectedAction;

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
                EditorGUI.LabelField(rect, new GUIContent(tour.title));
            };
            OnTourChanged(tourList);
        }

        private GuidedTour CreateNew(ReorderableList list, GuidedTourGraph graph, string defaultName = "")
        {
            var tour = GuidedTour.CreateInstance<GuidedTour>();
            graph.tours.Add(tour);
            if (graph.tours.Count == 1)
            {
                tour.name = tour.title = "default";
            }
            else
            {
                string[] tourNames = scene.graph?.tours.Select(tour => tour.name).ToArray();
                tour.title = tour.name = ObjectNames.GetUniqueName(tourNames, defaultName);
            }
            list.index = list.count - 1;
            OnTourChanged(list);
            this.Repaint();
            tourListScrollPosition.y = float.MaxValue;
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
            bool insertAfterCurrent = false;
            actionList = new ReorderableList(scene.graph.tours[tourList.index].actions, typeof(ScriptableActionCollection));
            actionList.headerHeight = 0;
            actionList.displayAdd = actionList.displayRemove = false;
            actionList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                ScriptableAction action = (ScriptableAction)scene.graph.tours[tourList.index].actions[index];
                EditorGUI.LabelField(rect, new GUIContent(ObjectNames.NicifyVariableName(action.Type)));
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
                        if (GUILayout.Button(EditorGUIUtility.TrIconContent("d_Toolbar Plus More"), EditorStyles.label, GUILayout.Width(18)))
                        {
                            GenericMenu menu = new GenericMenu();
                            menu.AddItem(new GUIContent("New"), false, () =>
                            {
                                CreateNew(tourList, scene.graph, "New Tour");
                                serializedTour.Update();
                            });
                            menu.AddItem(new GUIContent("Quick Tour/Navigate Through Hotspots"), false, () =>
                            {
                                GuidedTour tour = CreateNew(tourList, scene.graph, "Quick Tour");
                                foreach (var hotspot in scene.Hotspots)
                                {
                                    tour.actions.Add(new GoToAction()
                                    {
                                        target = hotspot
                                    });
                                    tour.actions.Add(new SetCameraRotationAction()
                                    {
                                        rotation = hotspot.Camera.transform.rotation
                                    });
                                    if (!hotspot.actionGraph.TryGetActionGroup("onSelect", out var onSelect)) continue;
                                    foreach (var action in onSelect)
                                    {
                                        tour.actions.Add(action);
                                    }
                                }
                                serializedTour.Update();
                                serializedTour.ApplyModifiedProperties();
                            });
                            menu.ShowAsContext();
                        }
                        if (GUILayout.Button(EditorGUIUtility.TrIconContent("d_Toolbar Minus"), EditorStyles.label, GUILayout.Width(18)))
                        {
                            if(tourList.index > -1 && tourList.index < tourList.count)
                            {
                                tourList.list.RemoveAt(tourList.index);
                                serializedTour = null;
                                if (tourList.index >= tourList.count)
                                {
                                    tourList.index = tourList.count - 1;
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
                            if (GUILayout.Button(EditorGUIUtility.TrIconContent("d_Toolbar Plus More"), EditorStyles.label, GUILayout.Width(18)))
                            {
                                var actionTypes = UnityEditor.TypeCache.GetTypesDerivedFrom<ScriptableAction>().Where(x => !x.IsAbstract && !x.IsDefined(typeof(ObsoleteAttribute), true));
                                GenericMenu menu = new GenericMenu();
                                foreach (var type in actionTypes)
                                {
                                    CategoryAttribute attribute = (CategoryAttribute)Attribute.GetCustomAttribute(type, typeof(CategoryAttribute));
                                    string nicfyName = GetNicifyActionName(type);
                                    menu.AddItem(new GUIContent(attribute != null ? $"{attribute.Path}/{nicfyName}" : nicfyName), false, () =>
                                    {
                                        scene.graph.tours[tourList.index].actions.Add(Activator.CreateInstance(type));
                                    //SerializedProperty element = actionList.serializedProperty.InsertManagedObject(type, insertAfterCurrent ? actionList.index : actionList.count);
                                    //element.isExpanded = element.managedReferenceFullTypename.EndsWith("GroupAction") ? false : true;
                                    //element.serializedObject.ApplyModifiedProperties();
                                    actionList.index++;
                                    });
                                    //CreateActionGroupAttribute[] linkedActionAttributes = (CreateActionGroupAttribute[])Attribute.GetCustomAttributes(type, typeof(CreateActionGroupAttribute));
                                    //if (linkedActionAttributes != null && linkedActionAttributes.Length > 0)
                                    //{
                                    //    foreach (var linkedActionAttribute in linkedActionAttributes)
                                    //    {
                                    //        StringBuilder pathBuilder = new StringBuilder();
                                    //        pathBuilder.Append(attribute != null ? $"{attribute.Path}/{nicfyName}(Group)" : $"{nicfyName}(Group)/");
                                    //        for (int i = 0; i < linkedActionAttribute.Types.Length; i++)
                                    //        {
                                    //            if (i > 0)
                                    //            {
                                    //                pathBuilder.Append(", ");
                                    //            }
                                    //            pathBuilder.Append(GetNicifyActionName(linkedActionAttribute.Types[i]));
                                    //        }
                                    //        menu.AddItem(new GUIContent(pathBuilder.ToString()), false, () =>
                                    //        {
                                    //            int index = actionList.index == -1 ? -1 : insertAfterCurrent ? actionList.index : actionList.count;
                                    //            SerializedProperty element = actionList.serializedProperty.InsertManagedObject(type, index++);
                                    //            foreach (var linkedType in linkedActionAttribute.Types)
                                    //            {
                                    //                actionList.serializedProperty.InsertManagedObject(linkedType, index++);
                                    //            }
                                    //            element.isExpanded = element.managedReferenceFullTypename.EndsWith("GroupAction") ? false : true;
                                    //            element.serializedObject.ApplyModifiedProperties();
                                    //            actionList.index = index > actionList.count - 1 ? actionList.count - 1 : index;
                                    //        });
                                    //    }
                                    //}
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
                        actionList?.DoLayoutList();
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
                    EditorGUI.indentLevel--;
                }
            }
                    serializedObject.ApplyModifiedProperties();
        }
    }
}
