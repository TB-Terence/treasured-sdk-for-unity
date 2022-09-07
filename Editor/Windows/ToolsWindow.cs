using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    public readonly struct GameObjectTriangle
    {
        public readonly string GameObjectName;
        public readonly int Triangles;
        public readonly GameObject GameObject;

        public GameObjectTriangle(string gameObjectName, int triangles, GameObject gameObject)
        {
            GameObjectName = string.IsNullOrWhiteSpace(gameObjectName) ? string.Empty : gameObjectName;
            Triangles = triangles;
            GameObject = gameObject;
        }
    }

    internal sealed class ToolsWindow : EditorWindow
    {
        private GUIStyle _labelStyle;
        private GUIStyle _boldLabelStyle;
        private GUIStyle _headerStyle;

        private Dictionary<int, GameObjectTriangle> _selectedGameObjectDict;
        private Vector2 _scrollPos = Vector2.zero;
        private bool _selectionMode = false;
        private MeshExporter _meshExporter;
        private int _totalTriangles = 0;

        [MenuItem("Tools/Treasured/Tools", priority = 0)]
        public static void ShowToolsWindow()
        {
            var window = GetWindow<ToolsWindow>();
            window.titleContent = new GUIContent("Treasured Tools");
            window.minSize = new Vector2(500, 500);
            // window._meshExporter = meshExporter;
            window.Show();
        }

        private void OnEnable()
        {
            var treasuredMapEditor = FindObjectOfType<TreasuredMap>();
            _meshExporter = treasuredMapEditor?.meshExporter;
            _selectedGameObjectDict = new Dictionary<int, GameObjectTriangle>();

            if (_labelStyle == null)
            {
                _labelStyle = new GUIStyle(EditorStyles.label)
                {
                    wordWrap = true,
                    richText = true,
                    alignment = TextAnchor.UpperCenter
                };
            }
            
            if (_boldLabelStyle == null)
            {
                _boldLabelStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    wordWrap = true,
                    richText = true,
                    fontSize = 14,
                    alignment = TextAnchor.UpperCenter
                };
            }

            if (_headerStyle == null)
            {
                _headerStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    wordWrap = true,
                    fontSize = 18,
                    alignment = TextAnchor.UpperCenter
                };
            }
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= SelectionChanged;
        }

        private void OnGUI()
        {
            if (EditorApplication.isCompiling)
            {
                return;
            }

            //  Heading
            EditorGUILayout.LabelField("Triangles Counter", _headerStyle);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            EditorGUILayout.Space();

            //  Triangle Count Mode Selection buttons
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(_selectionMode);
            if (GUILayout.Button("Selection Mode", GUI.skin.button))
            {
                Selection.selectionChanged += SelectionChanged;
                _selectionMode = true;

                //  Reset GameObject dictionary
                _selectedGameObjectDict.Clear();
                Repaint();
            }

            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("Analysis Export", GUI.skin.button))
            {
                Selection.selectionChanged -= SelectionChanged;
                _selectionMode = false;

                //  Reset GameObject dictionary
                _selectedGameObjectDict.Clear();

                if (_meshExporter == null)
                {
                    Debug.LogError(
                        "Error getting Reference to Mesh Export. Make sure TreasuredMap is present in the scene.");
                }
                else
                {
                    var meshForExport = _meshExporter.PrepareMeshForExport();
                    CalculateTriangles(meshForExport.Values.ToArray());
                }

                _totalTriangles = 0;
                foreach (var gameObjectTriangle in _selectedGameObjectDict.Values)
                {
                    _totalTriangles += gameObjectTriangle.Triangles;
                }

                //  Sort and display only top 10 highest Triangles count
                var sortedGameObjects = (from entry in _selectedGameObjectDict
                    orderby entry.Value.Triangles descending
                    select entry).Take(10).ToDictionary(pair => pair.Key, pair => pair.Value);
                _selectedGameObjectDict = sortedGameObjects;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(15);


            //  Mode Title
            EditorGUILayout.LabelField(_selectionMode ? "Object Selection Mode" : "Export Analysis Mode",
                _boldLabelStyle);


            if (_selectionMode)
            {
                _totalTriangles = 0;
                foreach (var gameObjectTriangle in _selectedGameObjectDict.Values)
                {
                    _totalTriangles += gameObjectTriangle.Triangles;
                }
            }

            if (_selectionMode)
            {
                EditorGUILayout.LabelField("Total Triangles for selected gameObjects:", _labelStyle);
            }
            else
            {
                EditorGUILayout.LabelField("Total Triangles in final export :", _labelStyle);
            }

            EditorGUILayout.LabelField($"{_totalTriangles}", _headerStyle);

            EditorGUILayout.Space(25);
            EditorGUILayout.LabelField("Detailed Breakdown:", _boldLabelStyle);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("GameObject", _boldLabelStyle);
            EditorGUILayout.LabelField("Triangles", _boldLabelStyle);
            EditorGUILayout.EndHorizontal();

            //  Contents
            using (var scroll = new EditorGUILayout.ScrollViewScope(_scrollPos))
            {
                _scrollPos = scroll.scrollPosition;

                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.BeginVertical();
                foreach (var selectedObjectName in _selectedGameObjectDict.Values)
                {
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                    if (!_selectionMode)
                    {
                        if (GUILayout.Button(selectedObjectName.GameObjectName, _labelStyle))
                        {
                            EditorGUIUtility.PingObject(selectedObjectName.GameObject);
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField(selectedObjectName.GameObjectName, _labelStyle);
                    }
                }

                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical();
                foreach (var selectedObjectTriangle in _selectedGameObjectDict.Values)
                {
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                    if (!_selectionMode)
                    {
                        if (GUILayout.Button(selectedObjectTriangle.Triangles.ToString(), _labelStyle))
                        {
                            EditorGUIUtility.PingObject(selectedObjectTriangle.GameObject);
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField(selectedObjectTriangle.Triangles.ToString(), _labelStyle);
                    }
                }

                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// Selection Changed callback
        /// </summary>
        private void SelectionChanged()
        {
            if (Selection.gameObjects.Length > 0)
            {
                //  Reset GameObject dictionary
                _selectedGameObjectDict.Clear();

                //  Focus on first selected Object
                LookAt(Selection.activeGameObject.transform);

                CalculateTriangles(Selection.gameObjects);

                Repaint();
            }
        }

        /// <summary>
        /// Calculate triangles of the selected objects
        /// </summary>
        /// <param name="gameObjects">Selected GameObjects</param>
        private void CalculateTriangles(GameObject[] gameObjects)
        {
            //  Loop through all the child gameObjects
            foreach (var selectedObject in gameObjects)
            {
                //  Only calculate if the object is active in scene
                if (selectedObject.activeSelf)
                {
                    var childTrianglesCount = 0;
                    if (_selectionMode)
                    {
                        foreach (var child in selectedObject.transform.GetComponentsInChildren<Transform>())
                        {
                            if (child.TryGetComponent(out MeshFilter meshFilter))
                            {
                                if (meshFilter.sharedMesh != null)
                                {
                                    childTrianglesCount += meshFilter.sharedMesh.triangles.Length;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (selectedObject.TryGetComponent(out MeshFilter meshFilter))
                        {
                            if (meshFilter.sharedMesh != null)
                            {
                                childTrianglesCount += meshFilter.sharedMesh.triangles.Length;
                            }
                        }
                    }

                    _selectedGameObjectDict.Add(selectedObject.GetInstanceID(),
                        new GameObjectTriangle(selectedObject.name, childTrianglesCount, selectedObject));
                }
                //  Object is not active in scene
                else
                {
                    _selectedGameObjectDict.Add(selectedObject.GetInstanceID(),
                        new GameObjectTriangle($"{selectedObject.name}-[DISABLED]", 0, selectedObject));
                }
            }
        }

        /// <summary>
        /// Look At Selected Object
        /// </summary>
        /// <param name="transform">Transform of the selected object</param>
        private void LookAt(Transform transform)
        {
            if (transform == null)
            {
                throw new ArgumentNullException(nameof(transform));
            }

            SceneView.lastActiveSceneView.LookAt(transform.position);
        }
    }
}
