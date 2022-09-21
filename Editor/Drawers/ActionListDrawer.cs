using System;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Treasured.UnitySdk
{
    internal class ActionListDrawer<T> where T : class
    {
        class Styles
        {
            public static readonly GUIStyle ToolbarButton = new GUIStyle(EditorStyles.boldLabel)
            {
                margin = new RectOffset(4, 4, 0, 0),
                padding = new RectOffset(4, 4, 0, 0),
                hover = new GUIStyleState()
                {
                    textColor = Color.red
                },
                normal = new GUIStyleState()
                {
                    textColor = Color.white
                }
            };
        }
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
                        _enabledAll = EditorGUI.ToggleLeft(new Rect(rect.x, rect.y, rect.xMax - 40, rect.height), new GUIContent(Header, $"{(_enabledAll ? "Disable" : "Enable")} all"), _enabledAll);
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
                        if (GUI.Button(new Rect(rect.xMax - 40, rect.y, 40, rect.height), new GUIContent("Clear", "Remove all actions"), Styles.ToolbarButton))
                        {
                            elements.ClearArray();
                            elements.serializedObject.ApplyModifiedProperties();
                            reorderableList.DoLayoutList(); // hacky way of resolving array index out of bounds error after clear.
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
                        if (typeof(ScriptableAction).IsAssignableFrom(typeof(T))) // TODO: Remove this after migrate to GuidedTourV2
                        {
                            Rect buttonRect = new Rect(rect.x, rect.y, 25, EditorGUIUtility.singleLineHeight);
                            SerializedProperty enabled = element.FindPropertyRelative("enabled");
                            EditorGUI.BeginChangeCheck();
                            enabled.boolValue = EditorGUI.ToggleLeft(buttonRect, new GUIContent(ObjectNames.NicifyVariableName(name)), enabled.boolValue);
                            if (EditorGUI.EndChangeCheck())
                            {
                                UpdateToggleState(elements);
                            }
                            EditorGUI.PropertyField(new Rect(rect.x + 25, rect.y, rect.width - 25, rect.height), element, new GUIContent(ObjectNames.NicifyVariableName(name)), true);
                        }
                        else
                        {
                            EditorGUI.PropertyField(rect, element, new GUIContent(ObjectNames.NicifyVariableName(name)), true);
                        }
                    }
                },
                elementHeightCallback = (int index) =>
                {
                    return EditorGUI.GetPropertyHeight(elements.GetArrayElementAtIndex(index), true);
                },
                onAddDropdownCallback = (Rect buttonRect, ReorderableList list) =>
                {
                    var actionTypes = TypeCache.GetTypesDerivedFrom<T>().Where(x => !x.IsAbstract && !x.IsDefined(typeof(ObsoleteAttribute), true));
                    GenericMenu menu = new GenericMenu();
                    foreach (var type in actionTypes)
                    {
                        CategoryAttribute attribute = (CategoryAttribute)Attribute.GetCustomAttribute(type, typeof(CategoryAttribute));
                        string nicfyName = GetNicifyActionName(type);
                        menu.AddItem(new GUIContent(attribute != null ? $"{attribute.Path}/{nicfyName}" : nicfyName), false, () =>
                        {
                            SerializedProperty element = elements.AppendManagedObject(type);
                            element.isExpanded = true;
                            element.serializedObject.ApplyModifiedProperties();
                        });
                        LinkedActionAttribute[] linkedActionAttributes = (LinkedActionAttribute[])Attribute.GetCustomAttributes(type, typeof(LinkedActionAttribute));
                        if (linkedActionAttributes != null && linkedActionAttributes.Length > 0)
                        {
                            foreach (var linkedActionAttribute in linkedActionAttributes)
                            {
                                StringBuilder pathBuilder = new StringBuilder();
                                pathBuilder.Append(attribute != null ? $"{attribute.Path}/{nicfyName}(Linked)" : $"{nicfyName}(Linked)/");
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
                                    SerializedProperty element = elements.AppendManagedObject(type);
                                    foreach (var linkedType in linkedActionAttribute.Types)
                                    {
                                        elements.AppendManagedObject(linkedType);
                                    }
                                    element.isExpanded = true;
                                    element.serializedObject.ApplyModifiedProperties();
                                });
                            }
                        }
                    }
                    menu.ShowAsContext();
                },
                onRemoveCallback = (ReorderableList list) =>
                {
                    elements.RemoveElementAtIndex(list.index);
                }
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
