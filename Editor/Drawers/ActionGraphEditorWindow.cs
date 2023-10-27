using System;
using UnityEngine;
using UnityEditor;
using Treasured.Actions;
using UnityEditorInternal;
using System.Collections.Generic;
using System.Linq;
using Treasured.UnitySdk.Utilities;
using TypeCache = UnityEditor.TypeCache;
using System.Text;
using System.Reflection;

namespace Treasured.UnitySdk
{
    class ActionGraphEditorWindow : EditorWindow
    {
        public static ActionGraphEditorWindow ShowWindow(SerializedProperty actionGraph)
        {
            var window = EditorWindow.GetWindow<ActionGraphEditorWindow>();
            window.actionGraph = actionGraph;
            window.titleContent = new GUIContent("Treasured Action Graph Editor", Resources.Load<Texture2D>("Treasured_Logo"));
            window.minSize = new Vector2(200, 300);
            window.Init();
            window.Show();
            return window;
        }

        SerializedProperty actionGraph;

        int selectedIndex;
        Tab[] tabs;

        class Tab
        {
            public string name;
            public ReorderableList actionList;
            public SerializedProperty actions;
        }

        void Init()
        {
            SerializedProperty groups = actionGraph.FindPropertyRelative("_groups");
            tabs = new Tab[groups.arraySize];
            for (int i = 0; i < groups.arraySize; i++)
            {
                var collection = groups.GetArrayElementAtIndex(i); // onSelect
                var actions = collection.FindPropertyRelative("_actions");
                tabs[i] = new Tab()
                {
                    name = ObjectNames.NicifyVariableName(collection.FindPropertyRelative("name").stringValue),
                };
                tabs[i].actions = actions;
                var actionNames = new List<string>();
                for (int x = 0; x < actions.arraySize; x++)
                {
                    SerializedProperty element = actions.GetArrayElementAtIndex(x);
                    string name = GetNicifyActionName(element.managedReferenceFullTypename.Substring(element.managedReferenceFullTypename.LastIndexOf('.') + 1));
                    actionNames.Add(name);
                }
                tabs[i].actionList = new ReorderableList(actionNames, typeof(string));
                tabs[i].actionList.list = actionNames;
                tabs[i].actionList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    EditorGUI.LabelField(rect, (string)(tabs[selectedIndex].actionList.list[index]), EditorStyles.boldLabel);
                };
                tabs[i].actionList.drawHeaderCallback = (Rect rect) =>
                {
                    EditorGUI.LabelField(rect, "Actions");
                };
                tabs[i].actionList.onAddDropdownCallback = (Rect buttonRect, ReorderableList list) =>
                {
                    ShowAddMenu(false);
                };
                tabs[i].actionList.onRemoveCallback = (ReorderableList list) =>
                {
                    actions.RemoveElementAtIndex(list.index);
                };
                tabs[i].actionList.onReorderCallbackWithDetails = (ReorderableList list, int oldIndex, int newIndex) =>
                {
                    tabs[i].actions.MoveArrayElement(oldIndex, newIndex);
                };
            }
        }

        private void ShowAddMenu(bool insertAfterCurrent = true)
        {
            var actionTypes = TypeCache.GetTypesDerivedFrom<ScriptableAction>().Where(x => x.IsDefined(typeof(APIAttribute), true) && !x.IsAbstract && !x.IsDefined(typeof(ObsoleteAttribute), true));
            GenericMenu menu = new GenericMenu();
            foreach (var type in actionTypes)
            {
                CategoryAttribute attribute = (CategoryAttribute)Attribute.GetCustomAttribute(type, typeof(CategoryAttribute));
                string nicfyName = GetNicifyActionName(type);
                var reorderableList = tabs[selectedIndex].actionList;
                menu.AddItem(new GUIContent(attribute != null ? $"{attribute.Path}/{nicfyName}" : nicfyName), false, () =>
                {
                    SerializedProperty element = tabs[selectedIndex].actions.InsertManagedObject(type, insertAfterCurrent ? reorderableList.index : reorderableList.count);
                    element.isExpanded = element.managedReferenceFullTypename.EndsWith("GroupAction") ? false : true;
                    element.serializedObject.ApplyModifiedProperties();
                    reorderableList.list.Add(nicfyName);
                    reorderableList.index = reorderableList.list.Count - 1;
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
                            int index = reorderableList.index == -1 ? -1 : insertAfterCurrent ? reorderableList.index : reorderableList.count;
                            SerializedProperty element = reorderableList.serializedProperty.InsertManagedObject(type, index++);
                            foreach (var linkedType in linkedActionAttribute.Types)
                            {
                                reorderableList.serializedProperty.InsertManagedObject(linkedType, index++);
                            }
                            element.isExpanded = element.managedReferenceFullTypename.EndsWith("GroupAction") ? false : true;
                            element.serializedObject.ApplyModifiedProperties();
                            reorderableList.index = index > reorderableList.count - 1 ? reorderableList.count - 1 : index;
                        });
                    }
                }
            }
            menu.ShowAsContext();
        }

        private string GetNicifyActionName(Type type)
        {
            return GetNicifyActionName(type.Name);
        }

        private string GetNicifyActionName(string name)
        {
            if (name.EndsWith("Action") && name.Length > 6)
            {
                name = name.Substring(0, name.Length - 6);
            }
            if (name.Length > 1)
            {
                name = char.ToLower(name[0]) + name.Substring(1);
            }
            return ObjectNames.NicifyVariableName(name);
        }

        private void OnGUI()
        {
            selectedIndex = GUILayout.SelectionGrid(selectedIndex, tabs.Select(x => x.name).ToArray(), tabs.Length);
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUILayout.VerticalScope(GUILayout.ExpandHeight(true), GUILayout.Width(200)))
                {
                    tabs[selectedIndex].actionList.DoLayoutList();
                }
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.ExpandHeight(true)))
                {
                    if (tabs[selectedIndex].actionList.index < 0)
                    {
                        EditorGUILayout.LabelField("Not Selected", EditorStyles.centeredGreyMiniLabel, GUILayout.ExpandHeight(true));
                    }
                    else
                    {
                        SerializedProperty element = tabs[selectedIndex].actions.GetArrayElementAtIndex(tabs[selectedIndex].actionList.index);
                        EditorGUIUtils.DrawPropertyWithoutFoldout(element);
                    }
                }
            }
        }
    }
}
