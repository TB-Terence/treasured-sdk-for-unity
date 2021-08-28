using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Treasured.SDKEditor;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk.Editor
{
    [CustomEditor(typeof(TreasuredMap))]
    internal sealed partial class TreasuredMapEditor : TreasuredEditor<TreasuredMap>
    {
        private enum GroupToggleState
        {
            None,
            All,
            Mixed
        }

        private bool _showInfo = true;

        private bool _showHotspotManagement = true;
        private bool _showHotspotList = true;
        private bool _exportAllHotspots = true;
        private GroupToggleState _hotspotsGroupToggleState = GroupToggleState.All;

        private bool _showInteractableManagement = true;
        private bool _showInteractableList = true;
        private bool _exportAllInteractables = true;
        private GroupToggleState _interactablesGroupToggleState = GroupToggleState.All;

        private bool _showExportSettings = true;

        private SerializedProperty _title;
        private SerializedProperty _description;
        private SerializedProperty _loop;

        #region Hotspot Management
        private float _hotspotGroundOffset = 2;
        #endregion

        #region Export Settings
        private SerializedProperty _format;
        private SerializedProperty _quality;
        private SerializedProperty _outputDirectory;
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
            _outputDirectory = serializedObject.FindProperty(nameof(_outputDirectory));
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
            EditorGUI.indentLevel++;
            if (foldout)
            {
                using (new EditorGUILayout.VerticalScope())
                {
                    action.Invoke();
                }
            }
            EditorGUI.indentLevel--;
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
            DrawTObjectList(Target.Hotspots, "Hotspots", ref _showHotspotList, ref _exportAllHotspots, ref _hotspotsGroupToggleState);
        }

        private void DrawInteractableManagment()
        {
            DrawTObjectList(Target.Interactables, "Interactables", ref _showInteractableList, ref _exportAllInteractables, ref _interactablesGroupToggleState, 3);
        }

        private void DrawTObjectList(IList<TreasuredObject> objects, string foldoutName, ref bool foldout, ref bool exportAll, ref GroupToggleState groupToggleState, float distance = 0)
        {
            if (objects == null)
            {
                return;
            }
            //groupToggleState = objects.All(x => x.gameObject.activeSelf) ? GroupToggleState.All : objects.Any(x => x.gameObject.activeSelf) ? GroupToggleState.Mixed : GroupToggleState.None;
            using (new EditorGUILayout.VerticalScope())
            {
                foldout = EditorGUILayout.Foldout(foldout, new GUIContent($"{foldoutName} ({objects.Count})"), true);
                if (foldout)
                {
                    if (objects.All(x => !x.gameObject.activeSelf))
                    {
                        exportAll = false;
                        groupToggleState = GroupToggleState.None;
                    }
                    else if (objects.Any(x => !x.gameObject.activeSelf))
                    {
                        groupToggleState = GroupToggleState.Mixed;
                    }
                    else
                    {
                        exportAll = true;
                        groupToggleState = GroupToggleState.All;
                    }
                    EditorGUI.showMixedValue = groupToggleState == GroupToggleState.Mixed;
                    EditorGUI.BeginChangeCheck();
                    exportAll = EditorGUILayout.ToggleLeft(new GUIContent($"Select All"), exportAll);
                    if (EditorGUI.EndChangeCheck())
                    {
                        foreach (var obj in objects)
                        {
                            obj.gameObject.SetActive(exportAll);
                        }
                    }
                    EditorGUI.showMixedValue = false;
                    foreach (var obj in objects)
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUI.BeginChangeCheck();
                            bool active = EditorGUILayout.Toggle("", obj.gameObject.activeSelf, GUILayout.Width(20));
                            if (EditorGUI.EndChangeCheck())
                            {
                                obj.gameObject.SetActive(active);
                            }
                            obj.gameObject.name = EditorGUILayout.TextField(obj.gameObject.name);
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
                }
            }
        }

        private void DrawExportSettings()
        {
            EditorGUILayout.PropertyField(_format);
            EditorGUILayout.PropertyField(_quality);
            CustomEditorGUILayout.FolderField(_outputDirectory, new GUIContent("Output Directory"));
            _showInExplorer = EditorGUILayout.Toggle(new GUIContent("Show In Explorer", "Opens the output directory once the exporting is done if enabled."), _showInExplorer);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Export Panoramic Images", GUILayout.Height(24)))
                {
                    DirectoryInfo outputDirectory = Directory.CreateDirectory(Path.Combine(_outputDirectory.stringValue, $"{Target.Data.Title}"));
                    ExportPanoramicImages(outputDirectory.FullName);
                    if (_showInExplorer)
                    {
                        Application.OpenURL(outputDirectory.FullName);
                    }
                }
                if (GUILayout.Button("Export Json", GUILayout.Height(24)))
                {
                    DirectoryInfo outputDirectory = Directory.CreateDirectory(Path.Combine(_outputDirectory.stringValue, $"{Target.Data.Title}"));
                    ExportJson(outputDirectory.FullName);
                    if (_showInExplorer)
                    {
                        Application.OpenURL(outputDirectory.FullName);
                    }
                }
                if (GUILayout.Button("Export All", GUILayout.Height(24)))
                {
                    DirectoryInfo outputDirectory = Directory.CreateDirectory(Path.Combine(_outputDirectory.stringValue, $"{Target.Data.Title}"));
                    ExportAll(outputDirectory.FullName);
                    if (_showInExplorer)
                    {
                        Application.OpenURL(outputDirectory.FullName);
                    }
                }
            }
        }
    }
}
