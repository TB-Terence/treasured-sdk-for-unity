using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Treasured.SDK;
using UnityEditor;
using UnityEngine;

namespace Treasured.SDKEditor
{
    [CustomEditor(typeof(TreasuredMap))]
    internal sealed class TreasuredMapEditor : TreasuredEditor<TreasuredMap>
    {
        private bool _showInfo = true;
        private bool _showHotspotManagement = true;
        private bool _showInteractableManagement = true;
        private bool _showExportSettings = true;

        private SerializedProperty _title;
        private SerializedProperty _description;
        private SerializedProperty _loop;
        #region Scene View
        private bool _showPreview = true;
        #endregion

        #region Hotspot Management
        private float _hotspotGroundOffset = 2;
        #endregion

        #region Export Settings
        private SerializedProperty _format;
        private SerializedProperty _quality;
        private SerializedProperty _exportDirectory;
        private bool _showInExplorer = true;
        #endregion

        protected override void Init()
        {
            Target.transform.hideFlags = HideFlags.HideInInspector;
            InitSerializedProperty();
            Tools.hidden = true;
        }

        private void OnDisable()
        {
            Tools.hidden = false;
        }

        private void InitSerializedProperty()
        {
            _title = serializedObject.FindProperty($"_data.{nameof(_title)}");
            _description = serializedObject.FindProperty($"_data.{nameof(_description)}");
            _loop = serializedObject.FindProperty($"_data.{nameof(_loop)}");
            _format = serializedObject.FindProperty($"_data.{nameof(_format)}");
            _quality = serializedObject.FindProperty($"_data.{nameof(_quality)}");
            _exportDirectory = serializedObject.FindProperty(nameof(_exportDirectory));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            using (new EditorGUILayout.VerticalScope())
            {
                DrawFoldoutGroup(ref _showInfo, new GUIContent("Info"), DrawInfo);
                DrawFoldoutGroup(ref _showHotspotManagement, new GUIContent("Hotspot Management"), DrawHotspotManagement);
                DrawFoldoutGroup(ref _showInteractableManagement, new GUIContent("Interactable Management"), DrawInteractableManagment);
                DrawFoldoutGroup(ref _showExportSettings, new GUIContent("Export"), DrawExportSettings);
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawFoldoutGroup(ref bool foldout, GUIContent label, Action action)
        {
            foldout = EditorGUILayout.BeginFoldoutHeaderGroup(foldout, label);
            if (foldout)
            {
                using (new EditorGUILayout.VerticalScope())
                {
                    action.Invoke();
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawInfo()
        {
            EditorGUILayout.PropertyField(_title);
            EditorGUILayout.PropertyField(_description);
            EditorGUILayout.PropertyField(_loop);
        }

        private void DrawHotspotManagement()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                _hotspotGroundOffset = EditorGUILayout.Slider(new GUIContent("Ground Offset for All", "Offset all Hotspots off the ground by this amount."), _hotspotGroundOffset, 0, 100);
                if (GUILayout.Button("Overwrite", GUILayout.Width(72)))
                {
                    foreach (var hotspot in Target.Hotspots)
                    {
                        if(hotspot.FindGroundPoint(100, ~0, out Vector3 ground))
                        {
                            hotspot.transform.position = ground + new Vector3(0, _hotspotGroundOffset, 0);
                            hotspot.OffsetHitbox();
                        }
                    }
                }
            }
            DrawTObjectList(Target.Hotspots);
        }

        private void DrawInteractableManagment()
        {
            DrawTObjectList(Target.Interactables, 3);
        }

        private void DrawTObjectList(IEnumerable<TObject> objects, float distance = 0)
        {
            if (objects == null)
            {
                return;
            }
            using (new EditorGUILayout.VerticalScope())
            {
                EditorGUI.indentLevel++;
                foreach (var obj in objects)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUI.BeginChangeCheck();
                        bool active = EditorGUILayout.ToggleLeft(obj.gameObject.name, obj.gameObject.activeSelf);
                        if (EditorGUI.EndChangeCheck())
                        {
                            //obj.gameObject.SetActive(_exportables[obj.Id]);
                            obj.gameObject.SetActive(active);
                        }
                        using(new EditorGUI.DisabledGroupScope(!obj.gameObject.activeSelf))
                        {
                            if (GUILayout.Button(EditorGUIUtility.TrIconContent("SceneViewCamera", "Move scene view to target"), EditorStyles.label, GUILayout.Width(20), GUILayout.Height(20)))
                            {
                                obj.transform.MoveSceneView(distance);
                                _showPreview = false;
                            }
                        }
                        if (GUILayout.Button(EditorGUIUtility.TrIconContent("Search Icon", "Select the game object in the hierarchy"), EditorStyles.label, GUILayout.Width(20), GUILayout.Height(20)))
                        {
                            Selection.activeGameObject = obj.gameObject;
                        }
                    }
                }
                EditorGUI.indentLevel--;
            }
        }

        private void DrawExportSettings()
        {
            EditorGUILayout.PropertyField(_format);
            EditorGUILayout.PropertyField(_quality);
            CustomEditorGUILayout.FolderField(_exportDirectory, new GUIContent("Export Directory"));
            _showInExplorer = EditorGUILayout.Toggle(new GUIContent("Show In Explorer", "Opens the output directory once the exporting is done if enabled."), _showInExplorer);
            if (GUILayout.Button("Export"))
            {
                Export();
            }
        }

        protected override void OnSceneGUI()
        {
            HandleUtility.AddDefaultControl(0);
            if (Event.current.type == EventType.MouseDown)
            {
                _showPreview = true;
            }
            if (!_showPreview)
            {
                return;
            }
            Vector3 hotspotSize = Vector3.one * 0.3f;
            for (int i = 0; i < Target.Hotspots.Length; i++)
            {
                Hotspot current = Target.Hotspots[i];
                if (!current.gameObject.activeSelf)
                {
                    continue;
                }
                Hotspot next = GetNextHotspot(i, Target.Hotspots.Length);
                switch (Tools.current)
                {
                    //case Tool.View:
                    //    EditorGUI.BeginChangeCheck();
                    //    Vector3 newPosition = Handles.PositionHandle(Target.transform.position, Quaternion.identity);
                    //    if (EditorGUI.EndChangeCheck())
                    //    {
                    //        Undo.RecordObject(Target.transform, "Move Map Position");
                    //        Target.transform.position = newPosition;
                    //    }
                    //    break;
                    case Tool.Move:
                        EditorGUI.BeginChangeCheck();
                        Vector3 newHotspotPosition = Handles.PositionHandle(current.transform.position, current.transform.rotation);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(current.transform, "Move Hotspot Position");
                            current.transform.position = newHotspotPosition;
                        }
                        break;
                    case Tool.Rotate:
                        EditorGUI.BeginChangeCheck();
                        Quaternion newHotspotRotation = Handles.RotationHandle(current.transform.rotation, current.transform.position);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(current.transform, "Edit Hotspot Rotation");
                            current.transform.rotation = newHotspotRotation;
                        }
                        float size = HandleUtility.GetHandleSize(current.transform.position);
                        Handles.color = Color.blue;
                        Handles.ArrowHandleCap(0, current.transform.position + current.transform.forward * size, current.transform.rotation, size, EventType.Repaint);
                        break;
                }
                Handles.color = Color.red;
                Handles.DrawWireCube(current.transform.position, hotspotSize);
                Handles.color = Color.white;
                if (current.Hitbox)
                {
                    Handles.DrawDottedLine(current.transform.position, current.transform.position + current.Hitbox.center, 5);
                }
                Handles.Label(current.transform.position, new GUIContent(current.gameObject.name));

                if (next)
                {
                    Vector3 direction = next.transform.position - current.transform.position;
                    if (direction != Vector3.zero)
                    {
                        Handles.color = Color.green;
                        Handles.ArrowHandleCap(0, current.transform.position, Quaternion.LookRotation(direction), 0.5f, EventType.Repaint);
                        //draw multiple arrows
                        //int segement = (int)(direction.magnitude / 3);
                        //for (int x = 0; x < segement; x++)
                        //{
                        //    Handles.ArrowHandleCap(0, current.transform.position + direction.normalized * x * direction.magnitude / segement, Quaternion.LookRotation(direction), 1, EventType.Repaint);
                        //}
                    }
                    Handles.color = Color.white;
                    Handles.DrawLine(current.transform.position, next.transform.position);
                }
            }
        }

        private Hotspot GetNextHotspot(int currentIndex, int totalCount)
        {
            int index = currentIndex;
            Hotspot current = Target.Hotspots[index];
            Hotspot next = Target.Hotspots[(index + 1) % totalCount];
            while (next != current)
            {
                if (next.gameObject.activeSelf)
                {
                    return next;
                }
                next = Target.Hotspots[++index % totalCount];
            }
            return null;
        }

        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ContractResolver = CustomContractResolver.Instance
        };

        private void Export()
        {
            if (string.IsNullOrEmpty(_exportDirectory.stringValue))
            {
                return;
            }
            Target.Data.GenerateHotspots(Target.Hotspots);
            Target.Data.GenerateInteractables(Target.Interactables);
            DirectoryInfo outputDirectory = Directory.CreateDirectory(Path.Combine(_exportDirectory.stringValue, $"{Target.Data.Title}"));
            string json = JsonConvert.SerializeObject(Target.Data, Formatting.Indented, JsonSettings);
            File.WriteAllText(Path.Combine(outputDirectory.FullName, $"map.json"), json);
            if (_showInExplorer)
            {
                Application.OpenURL(outputDirectory.FullName);
            }
        }
    }
}
