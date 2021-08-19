﻿using UnityEngine;
using UnityEditor;
using Treasured.SDK;
using System;
using System.Collections.Generic;

namespace Treasured.SDKEditor
{
    [CustomEditor(typeof(TreasuredData))]
    internal partial class TreasuredDataEditor : Editor
    {
        [SerializeField]
        private bool _showPreview;
        [SerializeField]
        private TreasuredData _data;

        private string[] _objectTabs = new string[] { "Interactables", "Hotspots" };
        private int _selectedTab = 0;

        private Vector2 _hotspotScrollPosition;
        private Vector2 _interactableScrollPosition;

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

        private void OnDisable()
        {
            GameObject.DestroyImmediate(TreasuredDataPreviewer.Instance.gameObject);
        }

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open in Editor", GUILayout.Height(24)))
            {
                TreasuredDataEditorWindow.Open(target as TreasuredData);
            }
            return;
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUI.BeginChangeCheck();
            _showPreview = GUILayout.Toggle(_showPreview, "Preview in Scene(Experimental)");
            if (EditorGUI.EndChangeCheck())
            {
                if(_showPreview)
                {
                    TreasuredDataPreviewer.Instance.Data = (TreasuredData)target;
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

            serializedObject.Update();
            SerializedProperty interactableProp = serializedObject.FindProperty("_interactables");
            SerializedProperty hotspotProp = serializedObject.FindProperty("_hotspots");
            _selectedTab = GUILayout.SelectionGrid(_selectedTab, _objectTabs, _objectTabs.Length, GUILayout.Height(26));
            CreateDropZone(_selectedTab);
            serializedObject.ApplyModifiedProperties();
        }

        private void CreateDropZone(int index)
        {
            string name = index == 0 ? "_hotspots" : "_interactables";
            SerializedProperty property = serializedObject.FindProperty(name);
            property.serializedObject.Update();
            EditorGUILayout.BeginVertical(GUI.skin.box);
            Rect rect = GUILayoutUtility.GetRect(0, 60);
            EditorGUILayout.Space(2);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Create New", GUILayout.Height(24)))
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
            }
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
