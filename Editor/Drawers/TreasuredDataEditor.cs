using UnityEngine;
using UnityEditor;
using Treasured.SDK;
using System.Linq;
using UnityEditor.SceneManagement;

namespace Treasured.SDKEditor
{
    [CustomEditor(typeof(TreasuredData))]
    internal partial class TreasuredDataEditor : Editor
    {
        [SerializeField]
        private bool _showPreview;
        [SerializeField]
        private TreasuredData _data;

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
        }

        private void OnDisable()
        {
            GameObject.DestroyImmediate(TreasuredDataPreviewer.Instance.gameObject);
        }

        public override void OnInspectorGUI()
        {
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
            using (new EditorGUI.DisabledGroupScope(EditorApplication.isPlaying))
            {
                if (GUILayout.Button(new GUIContent("Clear Object References", "Remove all Treasured Object References in scene."), EditorStyles.toolbarButton))
                {
                    foreach (var reference in GameObject.FindObjectsOfType<TreasuredObjectReference>())
                    {
                        GameObject.DestroyImmediate(reference);
                    }
                }
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
            iterator.NextVisible(true);
            while (iterator.NextVisible(false))
            {
                if (iterator.name == "m_Script")
                {
                    continue;
                }
                if (iterator.name == "_hotspots" || iterator.name == "_interactables")
                {
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    Rect rect = GUILayoutUtility.GetRect(0, 60);
                    EditorGUILayout.Space(2);
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(iterator);
                    EditorGUI.indentLevel--;
                    CustomEditorGUILayout.CreateDropZone(rect, new GUIContent($"{iterator.displayName} Drop Zone", $"Drag & Drop game object from hierarchy to add it to {iterator.displayName}."), (objects) =>
                    {
                        foreach (var obj in objects)
                        {
                            GameObject go = obj as GameObject;
                            if (!go)
                            {
                                continue;
                            }
                            if (go.TryGetComponent<TreasuredObjectReference>(out var reference))
                            {
                                if (string.IsNullOrEmpty(reference.Id))
                                {
                                    DestroyImmediate(reference);
                                    reference = go.AddComponent<TreasuredObjectReference>();
                                }
                                if (_data.Hotspots.Any(x => x.Id == reference.Id))
                                {
                                    Debug.Log($"Can't add {{{go.name}}} to {{{iterator.displayName}}}. Reference for {{{go.name}}} already exists in Hotspots.");
                                    continue;
                                }
                                if (_data.Interactables.Any(x => x.Id == reference.Id))
                                {
                                    Debug.Log($"Can't to add object {{{go.name}}} to {{{iterator.displayName}}}. Reference for {{{go.name}}} already exists in Interactables.");
                                    continue;
                                }
                            }
                            else
                            {
                                reference = go.AddComponent<TreasuredObjectReference>();
                            }
                            EditorUtility.SetDirty(go);
                            iterator.InsertArrayElementAtIndex(iterator.arraySize);
                            SerializedProperty p = iterator.GetArrayElementAtIndex(iterator.arraySize - 1);
                            p.FindPropertyRelative("_name").stringValue = obj.name;
                            p.FindPropertyRelative("_id").stringValue = reference.Id;
                            p.FindPropertyRelative("_transform._position").vector3Value = go.transform.position;
                            p.FindPropertyRelative("_transform._rotation").vector3Value = go.transform.rotation.eulerAngles;
                            if(go.TryGetComponent<Collider>(out var collider))
                            {
                                p.FindPropertyRelative("_hitbox._center").vector3Value = collider.bounds.center;
                                p.FindPropertyRelative("_hitbox._size").vector3Value = collider.bounds.size;
                            }
                            p.isExpanded = true;
                        }

                        iterator.serializedObject.ApplyModifiedProperties();
                        iterator.serializedObject.Update();
                    });
                    EditorGUILayout.EndVertical();
                }
                else
                {
                    EditorGUILayout.PropertyField(iterator);
                }
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
            serializedObject.ApplyModifiedProperties();
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
