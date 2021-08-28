using UnityEngine;
using UnityEditor;
using Treasured.UnitySdk;
using UnityEditor.SceneManagement;
using System;
using System.Collections.Generic;
using Treasured.SDK;

namespace Treasured.SDKEditor
{
    [CustomEditor(typeof(TreasuredData))]
    internal partial class TreasuredDataEditor : Editor
    {
        /// <summary>
        /// Current editing data.
        /// </summary>
        public static readonly Dictionary<string, string> ObjectIds = new Dictionary<string, string>();

        private bool _showPreview;
        private static TreasuredData _data;

        private string[] _objectTabs = new string[] { "Hotspots", "Interactables" };
        private int _selectedTab = 0;

        private Vector2 _hotspotScrollPosition;
        private Vector2 _interactableScrollPosition;

        private float hotspotPositionYOverwrite = 0;

        private void OnEnable()
        {
            _data = target as TreasuredData;
            // Validate
            foreach (var hotspot in _data.Hotspots)
            {
                if (hotspot.Hitbox.Size == Vector3.zero)
                {
                    hotspot.Hitbox.Size = Vector3.one;
                }
            }
            foreach (var interactable in _data.Interactables)
            {
                if (interactable.Hitbox.Size == Vector3.zero)
                {
                    interactable.Hitbox.Size = Vector3.one;
                }
            }
            serializedObject.FindProperty("_hotspots").isExpanded = true;
            serializedObject.FindProperty("_interactables").isExpanded = true;
        }

        public static void RefreshObjectIDs()
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

        public override void OnInspectorGUI()
        {
            if (Event.current.type == EventType.Repaint && ObjectIds.Count != _data.Hotspots.Count + _data.Interactables.Count)
            {
                RefreshObjectIDs();
            }
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUI.BeginChangeCheck();
            _showPreview = GUILayout.Toggle(_showPreview, "Preview in Scene");
            if (EditorGUI.EndChangeCheck())
            {
                if (_showPreview)
                {
                    TreasuredDataPreviewer.Instance.Data = (TreasuredData)target;
                    Selection.activeGameObject = TreasuredDataPreviewer.Instance.gameObject;
                }
                else
                {
                    GameObject.DestroyImmediate(TreasuredDataPreviewer.Instance.gameObject);
                }
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(new GUIContent("Reset", "Reset all fields to default."), EditorStyles.toolbarButton))
            {
                Undo.RecordObject(target, "Reset Treasured Data");
                _data.Name = "";
                _data.Loop = false;
                _data.Hotspots.Clear();
                _data.Interactables.Clear();
                GUI.FocusControl(null); // reset control focus
            }
            Rect exportButtonRect = GUILayoutUtility.GetRect(new GUIContent("Export"), EditorStyles.toolbarButton);
            if (GUI.Button(exportButtonRect, "Export", EditorStyles.toolbarButton))
            {
                ShowExportConfigWindow(exportButtonRect);
            }
            EditorGUILayout.EndHorizontal();
            using (new EditorGUI.DisabledGroupScope(true))
            {
                EditorGUILayout.LabelField("Version", TreasuredData.Version);
            };
            serializedObject.Update();
            SerializedProperty iterator = serializedObject.GetIterator();
            iterator.isExpanded = EditorGUILayout.Foldout(iterator.isExpanded, new GUIContent("Metadata"), true);
            if (iterator.isExpanded)
            {
                EditorGUI.indentLevel++;
                iterator.NextVisible(true);
                while (iterator.NextVisible(false))
                {
                    if (iterator.name == "m_Script" || iterator.name == "_hotspots" || iterator.name == "_interactables")
                    {
                        continue;
                    }
                    EditorGUILayout.PropertyField(iterator);
                    if (iterator.name == "_quality" && iterator.enumValueIndex == 3)
                    {
                        EditorGUILayout.HelpBox("Use with caution!\n" +
                            "Ultra setting will use a lot of memory due to a bug with Unity.\n" +
                            "The memory is unlikely to be released until entering or exiting play mode and upon assembly reloaded.", MessageType.Warning);
                    }
                    if (iterator.name == "_name" && string.IsNullOrEmpty(iterator.stringValue))
                    {
                        string sceneName = EditorSceneManager.GetActiveScene().name;
                        EditorGUILayout.HelpBox($"The default output folder name will be '{sceneName}'.", MessageType.Warning);
                    }
                }
                EditorGUI.indentLevel--;
            }
            _selectedTab = GUILayout.SelectionGrid(_selectedTab, _objectTabs, _objectTabs.Length, GUILayout.Height(26));
            CreateDropZone(_selectedTab);
            serializedObject.ApplyModifiedProperties();
        }

        private void CreateDropZone(int index)
        {
            string name = index == 0 ? "_hotspots" : "_interactables";
            SerializedProperty property = serializedObject.FindProperty(name);
            //property.serializedObject.Update();
            EditorGUILayout.BeginVertical(GUI.skin.box);
            Rect rect = GUILayoutUtility.GetRect(0, 60);
            EditorGUILayout.Space(2);
            using (new EditorGUILayout.VerticalScope())
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button(new GUIContent("Create New", "Insert a new element with default value"), GUILayout.Height(24)))
                    {
                        property.InsertArrayElementAtIndex(property.arraySize);
                        SerializedProperty newElement = property.GetArrayElementAtIndex(property.arraySize - 1);
                        // insert a new element will copy the data from the previous element
                        newElement.FindPropertyRelative("_id").stringValue = Guid.NewGuid().ToString();
                        newElement.FindPropertyRelative("_name").stringValue = $"{(index == 0 ? "Hotspot" : "Interactable")} {property.arraySize}";
                        newElement.FindPropertyRelative("_description").stringValue = "";
                        newElement.FindPropertyRelative("_transform._position").vector3Value = Vector3.zero;
                        newElement.FindPropertyRelative("_transform._rotation").vector3Value = Vector3.zero;
                        newElement.FindPropertyRelative("_hitbox._center").vector3Value = Vector3.zero;
                        newElement.FindPropertyRelative("_hitbox._size").vector3Value = Vector3.one;
                        newElement.FindPropertyRelative("_onSelected").arraySize = 0;
                    }
                    if (GUILayout.Button("Clear All", GUILayout.Height(24)))
                    {
                        property.arraySize = 0;
                    }
                    if (GUILayout.Button("Collapse All", GUILayout.Height(24)))
                    {
                        for (int i = 0; i < property.arraySize; i++)
                        {
                            property.GetArrayElementAtIndex(i).isExpanded = false;
                        }
                        property.isExpanded = false;
                    }
                    if (GUILayout.Button("Expand All", GUILayout.Height(24)))
                    {
                        for (int i = 0; i < property.arraySize; i++)
                        {
                            property.GetArrayElementAtIndex(i).isExpanded = true;
                        }
                        property.isExpanded = true;
                    }
                    if (index == 0 && GUILayout.Button("Raycast Hitbox for All", GUILayout.Height(24)))
                    {
                        for (int i = 0; i < property.arraySize; i++)
                        {
                            SerializedProperty hotspotElementProp = property.GetArrayElementAtIndex(i);
                            SerializedProperty positionProp = hotspotElementProp.FindPropertyRelative("_transform._position");
                            SerializedProperty centerProp = hotspotElementProp.FindPropertyRelative("_hitbox._center");
                            SerializedProperty sizeProp = hotspotElementProp.FindPropertyRelative("_hitbox._size");
                            if (Physics.Raycast(positionProp.vector3Value, positionProp.vector3Value - positionProp.vector3Value + Vector3.down, out var hit, 100))
                            {
                                if (sizeProp.vector3Value == Vector3.zero)
                                {
                                    sizeProp.vector3Value = Vector3.one;
                                }
                                float sizeHeight = sizeProp.vector3Value.y;
                                centerProp.vector3Value = hit.point + new Vector3(0, sizeHeight / 2, 0);
                            }
                            else
                            {
                                Debug.LogWarning($"No hit found for {_data.Hotspots[i].Name}. Make sure the position is above ground and the ground has a collider component. Maximum distance: 100");
                            }
                        }
                    }
                }
                if (index == 0)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        hotspotPositionYOverwrite = EditorGUILayout.FloatField(new GUIContent("Hotspot Position-Y Overwrite"), hotspotPositionYOverwrite);
                        if (GUILayout.Button("Overwrite"))
                        {
                            for (int i = 0; i < property.arraySize; i++)
                            {
                                SerializedProperty position = property.GetArrayElementAtIndex(i).FindPropertyRelative("_transform._position");
                                position.vector3Value = new Vector3(position.vector3Value.x, hotspotPositionYOverwrite, position.vector3Value.z);
                            }
                            property.serializedObject.ApplyModifiedProperties();
                        }
                    }
                }
            }
            EditorGUILayout.HelpBox("The default behaviour of the add button will copy the last element which results duplicated id. Make sure to regenerate id after adding a new element to the list.\n Use the 'Create New' button above to insert a new object with default value.", MessageType.Warning);
            EditorGUI.indentLevel++;
            using (var scope = new EditorGUILayout.ScrollViewScope(index == 0 ? _hotspotScrollPosition : _interactableScrollPosition, GUILayout.MaxHeight(600)))
            {
                if (index == 0)
                {
                    _hotspotScrollPosition = scope.scrollPosition;
                }
                else if (index == 1)
                {
                    _interactableScrollPosition = scope.scrollPosition;
                }
                EditorGUILayout.PropertyField(property);
            }
            EditorGUI.indentLevel--;
            CustomEditorGUILayout.CreateDropZone(rect, new GUIContent($"{property.displayName} Drop Zone", $"Drag & Drop game object from hierarchy to add it to {property.displayName}."), (objects) =>
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
                    else
                    {
                        p.FindPropertyRelative("_hitbox._center").vector3Value = go.transform.position;
                        p.FindPropertyRelative("_hitbox._size").vector3Value = Vector3.one;
                    }
                    p.isExpanded = true;
                }

                property.serializedObject.ApplyModifiedProperties();
                property.serializedObject.Update();
            });
            EditorGUILayout.EndVertical();
        }

        Rect OnDrawList(SerializedProperty property)
        {
            Rect rect = EditorGUILayout.BeginVertical();
            property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, $"{property.displayName}({property.arraySize})", true);
            if (property.isExpanded)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Create New", EditorStyles.toolbarButton))
                {
                    property.arraySize++;
                }
                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel++;
                for (int i = 0; i < property.arraySize; i++)
                {
                    SerializedProperty element = property.GetArrayElementAtIndex(i);
                    EditorGUILayout.PropertyField(element);
                }
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();
            return rect;
        }
    }
}