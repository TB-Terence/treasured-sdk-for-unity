using System;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Treasured.UnitySdk
{
    internal class ActionListDrawer<T> where T : class
    {
        internal ReorderableList reorderableList;
        public string Header { get; set; }

        private bool _enabledAll = false;

        private TreasuredMapEditor.GroupToggleState _toggleState;

        public ActionListDrawer(SerializedObject serializedObject, SerializedProperty elements, string header)
        {
            Header = header;
            if (typeof(ScriptableAction).IsAssignableFrom(typeof(T))) // TODO: Remove this after migrate to GuidedTourV2
            {
                UpdateToggleState(elements);
            }
            reorderableList = new ReorderableList(serializedObject, elements)
            {
                drawHeaderCallback = (Rect rect) =>
                {
                    if (typeof(ScriptableAction).IsAssignableFrom(typeof(T))) // TODO: Remove this after migrate to GuidedTourV2
                    {
                        EditorGUI.BeginChangeCheck();
                        EditorGUI.showMixedValue = _toggleState == TreasuredMapEditor.GroupToggleState.Mixed;
                        _enabledAll = EditorGUI.ToggleLeft(new Rect(rect.x, rect.y, rect.xMax - 120, rect.height), new GUIContent(Header, $"{(_enabledAll ? "Disable" : "Enable")} all"), _enabledAll);
                        if (EditorGUI.EndChangeCheck())
                        {
                            for (int i = 0; i < elements.arraySize; i++)
                            {
                                SerializedProperty element = elements.GetArrayElementAtIndex(i);
                                SerializedProperty enabledProperty = element.FindPropertyRelative("enabled");
                                enabledProperty.boolValue = _enabledAll;
                            }
                            UpdateToggleState(elements);
                        }
                        bool disabled = elements.arraySize == 0;
                        //if (GUI.Button(new Rect(rect.xMax - 190, rect.y, 80, rect.height), new GUIContent("Expand All"), EditorStyles.boldLabel))
                        //{
                        //    ChangeExpandedState(true);
                        //    reorderableList.DoLayoutList();
                        //}
                        //if (GUI.Button(new Rect(rect.xMax - 120, rect.y, 80, rect.height), new GUIContent("Collapse All"), EditorStyles.boldLabel))
                        //{
                        //    ChangeExpandedState(false);
                        //    reorderableList.DoLayoutList();
                        //}
                        using (new EditorGUI.DisabledGroupScope(disabled))
                        {
                            if (GUI.Button(new Rect(rect.xMax - 40, rect.y, 40, rect.height), new GUIContent("Clear", "Remove all actions"), disabled ? EditorStyles.boldLabel : DefaultStyles.ClearButton))
                            {
                                elements.ClearArray();
                                elements.serializedObject.ApplyModifiedProperties();
                                reorderableList.DoLayoutList(); // hacky way of resolving array index out of bounds error after clear.
                            }
                        }
                        
                    }
                    else
                    {
                        EditorGUI.LabelField(rect, Header);
                    }
                },
                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    SerializedProperty element = elements.GetArrayElementAtIndex(index);
                    using (new EditorGUI.IndentLevelScope(1))
                    {
                        string name = element.managedReferenceFullTypename.Substring(element.managedReferenceFullTypename.LastIndexOf('.') + 1);
                        if (name.EndsWith("Action") && name.Length > 6)
                        {
                            name = name.Substring(0, name.Length - 6);
                        }
                        if (name.Length > 1)
                        {
                            name = char.ToLower(name[0]) + name.Substring(1);
                        }
                        name = ObjectNames.NicifyVariableName(name);
                        SerializedProperty targetProperty = element.FindPropertyRelative("target");
                        if (targetProperty != null && targetProperty.propertyType == SerializedPropertyType.ObjectReference)
                        {
                            if (!targetProperty.objectReferenceValue.IsNullOrNone())
                            {
                                name += $" ({targetProperty.objectReferenceValue.name})";;
                            }
                            else
                            {
                                name += $" (Not selected)";
                            }
                            EditorGUILayoutUtils.CreateDropZone(rect, DragAndDropVisualMode.Link, (targets) =>
                            {
                                if (targets.Length > 0)
                                {
                                    var target = targets[0];
                                    if (!target.IsNullOrNone())
                                    {
                                        targetProperty.objectReferenceValue = target;
                                        targetProperty.serializedObject.ApplyModifiedProperties();
                                    }
                                }
                            });
                        }
                        if (typeof(ScriptableAction).IsAssignableFrom(typeof(T))) // TODO: Remove this after migrate to GuidedTourV2
                        {
                            Rect buttonRect = new Rect(rect.x, rect.y, 25, EditorGUIUtility.singleLineHeight);
                            SerializedProperty enabled = element.FindPropertyRelative("enabled");
                            if(enabled != null)
                            {
                                EditorGUI.BeginChangeCheck();
                                enabled.boolValue = EditorGUI.ToggleLeft(buttonRect, new GUIContent(name), enabled.boolValue);
                                if (EditorGUI.EndChangeCheck())
                                {
                                    UpdateToggleState(elements);
                                }
                                EditorGUI.PropertyField(new Rect(rect.x + 25, rect.y, rect.width - 64, rect.height), element, new GUIContent(name), true);
                            }
                            else
                            {
                                EditorGUI.LabelField(new Rect(rect.x + 25, rect.y, rect.width - 64, rect.height), "Unknown Type. You should remove this item.");
                            }
                        }
                        else
                        {
                            EditorGUI.PropertyField(rect, element, new GUIContent(name), true);
                        }
                        if (GUI.Button(new Rect(rect.xMax - 40, rect.y, 20, EditorGUIUtility.singleLineHeight), EditorGUIUtility.TrIconContent("Toolbar Plus More", "Insert After"), EditorStyles.label))
                        {
                            ShowAddMenu();
                        }
                        if (GUI.Button(new Rect(rect.xMax - 20, rect.y, 20, EditorGUIUtility.singleLineHeight), EditorGUIUtility.TrIconContent("Toolbar Minus", "Remove"), EditorStyles.label))
                        {
                            elements.RemoveElementAtIndex(index);
                            reorderableList.DoLayoutList();
                        }
                    }
                },
                elementHeightCallback = (int index) =>
                {
                    return EditorGUI.GetPropertyHeight(elements.GetArrayElementAtIndex(index), true);
                },
                onAddDropdownCallback = (Rect buttonRect, ReorderableList list) =>
                {
                    ShowAddMenu(false);
                },
                onRemoveCallback = (ReorderableList list) =>
                {
                    elements.RemoveElementAtIndex(list.index);
                }
            };
        }

        private void ChangeExpandedState(bool expanded)
        {
            for (int i = 0; i < reorderableList.serializedProperty.arraySize; i++)
            {
                SerializedProperty element = reorderableList.serializedProperty.GetArrayElementAtIndex(i);
                element.isExpanded = expanded;
            }
        }

        private void ShowAddMenu(bool insertAfterCurrent = true)
        {
            var actionTypes = TypeCache.GetTypesDerivedFrom<T>().Where(x => !x.IsAbstract && !x.IsDefined(typeof(ObsoleteAttribute), true));
            GenericMenu menu = new GenericMenu();
            foreach (var type in actionTypes)
            {
                CategoryAttribute attribute = (CategoryAttribute)Attribute.GetCustomAttribute(type, typeof(CategoryAttribute));
                string nicfyName = GetNicifyActionName(type);
                menu.AddItem(new GUIContent(attribute != null ? $"{attribute.Path}/{nicfyName}" : nicfyName), false, () =>
                {
                    SerializedProperty element = reorderableList.serializedProperty.InsertManagedObject(type, insertAfterCurrent ? reorderableList.index : reorderableList.count);
                    element.isExpanded = element.managedReferenceFullTypename.EndsWith("GroupAction") ? false : true;
                    element.serializedObject.ApplyModifiedProperties();
                    reorderableList.index++;
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
                            element.isExpanded = element.managedReferenceFullTypename.EndsWith("GroupAction") ? false: true;
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
            string name = type.Name;
            if (name.EndsWith("Action") && name.Length > 6)
            {
                name = name.Substring(0, name.Length - 6);
            }
            return ObjectNames.NicifyVariableName(name);
        }

        public void OnGUI(Rect rect)
        {
            reorderableList.serializedProperty.serializedObject.Update();
            reorderableList.DoList(rect);
            reorderableList.serializedProperty.serializedObject.ApplyModifiedProperties();
        }

        public void OnGUILayout()
        {
            reorderableList.serializedProperty.serializedObject.Update();
            reorderableList.DoLayoutList();
            reorderableList.serializedProperty.serializedObject.ApplyModifiedProperties();
        }

        private void UpdateToggleState(SerializedProperty elements)
        {
            bool[] enabled = new bool[elements.arraySize];
            for (int i = 0; i < elements.arraySize; i++)
            {
                SerializedProperty element = elements.GetArrayElementAtIndex(i);
                SerializedProperty enabledProperty = element.FindPropertyRelative("enabled");
                if (enabledProperty == null)
                {
                    continue;
                }
                enabled[i] = enabledProperty.boolValue;
            }
            int enabledCount = enabled.Count(x => x == true);
            if (enabledCount == enabled.Length)
            {
                _toggleState = TreasuredMapEditor.GroupToggleState.All;
                _enabledAll = true;
            }
            else
            {
                _toggleState = enabledCount == 0 ? TreasuredMapEditor.GroupToggleState.None : TreasuredMapEditor.GroupToggleState.Mixed;
                _enabledAll = false;
            }
        }
    }
}
