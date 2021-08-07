using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using System.IO;
using System;
using Newtonsoft.Json;
using Treasured.ExhibitX;
using Newtonsoft.Json.Converters;
using Simple360Render;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Unity.EditorCoroutines.Editor;

namespace Treasured.ExhibitXEditor
{
    public class TreasuredToolkitWindow : EditorWindow, IHasCustomMenu
    {
        private static string[] _tabs = new string[] { "Hotspot Management", "Panoramas Exporter" };

        [MenuItem("Tools/Treasured Toolkit")]
        public static void OpenWindow()
        {
            var window = GetWindow<TreasuredToolkitWindow>();
            window.position = new Rect(200, 200, 324, 480);
            window.maxSize = new Vector2(324, 480);
            window.titleContent = new GUIContent("Treasured Toolkit", Styles.TreasuredIcon);
            window.Show();
        }

        private bool _selectAll;

        private Vector2 _hotspotScrollPosition;

        private string _outputFolder;
        private ImageQuality _quality = ImageQuality.Low;
        private ImageFormat _format = ImageFormat.JPEG;
        private bool _openFolderAfterExport = false;
        private bool _folderInAssetFolder = false;

        private bool _isExporting = false;

        private int _selectedTabIndex;

        private List<Hotspot> _hotspots = new List<Hotspot>();
        private float _newYForAll = 0;

        void OnEnable()
        {
            OnHierarchyChange();
        }

        private void OnHierarchyChange()
        {
            _hotspots.Clear();
            if (HotspotManager.Instance)
            {
                _hotspots.AddRange(HotspotManager.Instance.GetComponentsInChildren<Hotspot>(true));
            }
            this.Repaint();
        }

        private void OnGUI()
        {
            GUI.skin = null; // use default skin
            EditorGUI.BeginDisabledGroup(_isExporting);
            _selectedTabIndex = GUILayout.SelectionGrid(_selectedTabIndex, _tabs, _tabs.Length, EditorStyles.toolbarButton);
            switch (_selectedTabIndex)
            {
                case 0:
                    DrawHotspotManager();
                    break;
                case 1:
                    DrawExporter();
                    break;
            }
            EditorGUI.EndDisabledGroup();
        }

        private void DrawExporter()
        {
            EditorGUILayout.BeginHorizontal();
            float previousWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 100;
            EditorGUILayout.PrefixLabel("Output Folder", Styles.ShortLabel);
            if (GUILayout.Button(EditorGUIUtils.IconContent("Folder Icon", "Select Folder"), Styles.Label, GUILayout.Width(20), GUILayout.Height(20)))
            {
                _outputFolder = EditorUtility.OpenFolderPanel("Select folder", "", "");
                if (_outputFolder.StartsWith(Application.dataPath))
                {
                    _folderInAssetFolder = true;
                }
                else
                {
                    _folderInAssetFolder = false;
                }
            }
            EditorGUILayout.LabelField(new GUIContent(_outputFolder, _outputFolder), Styles.Label);
            EditorGUILayout.EndHorizontal();
            
            if (_folderInAssetFolder)
            {
                EditorGUILayout.HelpBox("Output folder under Assets folder is not recommanded due to Unity meta files.", MessageType.Warning);
            }
            _quality = (ImageQuality)EditorGUILayout.EnumPopup("Quality", _quality);
            _format = (ImageFormat)EditorGUILayout.EnumPopup("Format", _format);
            _openFolderAfterExport = EditorGUILayout.Toggle(new GUIContent("Open Folder", "Open output folder after export"), _openFolderAfterExport);
            GUILayout.FlexibleSpace();
            if (string.IsNullOrEmpty(_outputFolder))
            {
                EditorGUILayout.HelpBox("Output folder not selected.", MessageType.Error);
            }
            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(_outputFolder));
            if (GUILayout.Button("Export", GUILayout.Height(36)))
            {
                this.StartCoroutine(Export());
            }
            EditorGUI.EndDisabledGroup();
            EditorGUIUtility.labelWidth = previousWidth;
        }

        private void DrawHotspotManager()
        {
            if (HotspotManager.Instance == null)
            {
                if (GUILayout.Button("Create Manager", GUILayout.Height(36)))
                {
                    if (!HotspotManager.Instance)
                    {
                        GameObject go = new GameObject("Treasured Hotspot Manager");
                        go.AddComponent<HotspotManager>();
                    }
                }
                return;
            }
            EditorGUILayout.BeginVertical();
            {// manager states
                EditorGUI.BeginChangeCheck();
                HotspotManager.Instance.loop = EditorGUILayout.Toggle(new GUIContent("Loop", "If enabled, the last hotspot will connect to the first active hotspot."), HotspotManager.Instance.loop);
                if (EditorGUI.EndChangeCheck())
                {
                    SceneView.RepaintAll();
                }
            }
            EditorGUILayout.EndVertical();
            {// drop zone
                if (GUILayout.Button(new GUIContent("Click to create a new hotspot\nOR\nDrag & Drop object(s) from the scene to create a copy"), Styles.DropZone))
                {
                    Undo.RegisterFullObjectHierarchyUndo(HotspotManager.Instance, "Create new hotspot");
                    GameObject go = new GameObject();
                    go.transform.SetParent(HotspotManager.Instance.gameObject.transform);
                    go.AddComponent<Hotspot>();
                    go.name = $"Hotspot {go.transform.GetSiblingIndex()}";
                }
                Rect buttonRect = GUILayoutUtility.GetLastRect();
                EditorGUIUtility.AddCursorRect(buttonRect, MouseCursor.Link);
                EditorGUIUtils.CreateDropZone(buttonRect, () =>
                {
                    Undo.RegisterCompleteObjectUndo(HotspotManager.Instance, "Create copy for hotspot");
                    foreach (var obj in DragAndDrop.objectReferences)
                    {
                        if (obj is GameObject go && go.scene != null && go.transform != HotspotManager.Instance.transform && go.transform.parent != HotspotManager.Instance.transform)
                        {
                            GameObject newHotspot = new GameObject(go.name);
                            newHotspot.transform.SetParent(HotspotManager.Instance.transform);
                            newHotspot.AddComponent<Hotspot>();
                            newHotspot.transform.SetPositionAndRotation(go.transform.position, go.transform.rotation);
                            newHotspot.name = $"Hotspot {newHotspot.transform.GetSiblingIndex()}";
                        }
                    }
                });
            }
            
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            _selectAll = EditorGUILayout.Toggle(_selectAll);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (var hotspot in HotspotManager.Instance.GetComponentsInChildren<Hotspot>(true))
                {
                    hotspot.gameObject.SetActive(hotspot.enabled = _selectAll);
                }
            }
            EditorGUILayout.LabelField($"Targets({(_hotspots.Count)})");
            EditorGUILayout.LabelField("Actions");
            EditorGUILayout.EndHorizontal();
            _hotspotScrollPosition = GUILayout.BeginScrollView(_hotspotScrollPosition, GUILayout.Height(302));
            for (int index = 0; index < _hotspots.Count; index++)
            {
                Hotspot hotspot = _hotspots[index];
                if (hotspot == null)
                {
                    continue;
                }
                SerializedObject serializedObject = new SerializedObject(hotspot);
                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();
                hotspot.enabled = EditorGUILayout.Toggle(hotspot.isActiveAndEnabled, GUILayout.Width(18));
                if (EditorGUI.EndChangeCheck())
                {
                    hotspot.gameObject.SetActive(hotspot.enabled);
                }
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.ObjectField(hotspot, typeof(Hotspot), false, GUILayout.Width(200));
                EditorGUI.EndDisabledGroup();
                float newY = hotspot.transform.position.y;
                float previousLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 10;
                EditorGUI.BeginChangeCheck();
                newY = EditorGUILayout.Slider(new GUIContent("Y", "Adjust the y position of the hotspot"), hotspot.transform.position.y, -100000, 100000, GUILayout.Width(72));
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(hotspot.transform, "Modify hotspot y position");
                    hotspot.transform.position = new Vector3(hotspot.transform.position.x, newY, hotspot.transform.position.z);
                }
                if (GUILayout.Button(EditorGUIUtils.IconContent("_Menu", "Menu"), GUI.skin.label, GUILayout.MaxWidth(16)))
                {
                    CreateControlMenu(hotspot, index, HotspotManager.Instance.transform.childCount);
                }
                EditorGUIUtility.labelWidth = previousLabelWidth;
                EditorGUILayout.EndHorizontal();
                if (GUI.changed && serializedObject.targetObject)
                {
                    serializedObject.ApplyModifiedProperties();
                    serializedObject.Update();
                }
            }
            GUILayout.EndScrollView();
            EditorGUI.BeginDisabledGroup(_hotspots.Count == 0);
            if (GUILayout.Button(new GUIContent("Remove All", "Remove all hotspots")))
            {
                Undo.RegisterFullObjectHierarchyUndo(HotspotManager.Instance.gameObject, "Remove all hotspots");
                foreach (var hotspot in _hotspots)
                {
                    GameObject.DestroyImmediate(hotspot.gameObject);
                }
            }
            EditorGUI.BeginChangeCheck();
            _newYForAll = EditorGUILayout.Slider("Set y position for all", _newYForAll, -100000, 100000);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RegisterFullObjectHierarchyUndo(HotspotManager.Instance.gameObject, "Set y for all hotspot");
                foreach (var transform in GetHotspotEnumerable())
                {
                    transform.position = new Vector3(transform.position.x, _newYForAll, transform.position.z);
                }
            }
            EditorGUI.EndDisabledGroup();
        }

        private IEnumerable<Transform> GetHotspotEnumerable()
        {
            for (int i = 0; i < HotspotManager.Instance.transform.childCount; i++)
            {
                yield return HotspotManager.Instance.transform.GetChild(i);
            }
        }

        /// <summary>
        /// Create control menu for hotspot element
        /// </summary>
        /// <param name="hotspot"></param>
        private void CreateControlMenu(Hotspot hotspot, int index, int totalCount)
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Delete"), false, () =>
            {
                Undo.DestroyObjectImmediate(hotspot.gameObject);
            });
            menu.AddSeparator("");
            if (index == 0)
            {
                menu.AddDisabledItem(new GUIContent("Move Up"));
            }
            else
            {
                menu.AddItem(new GUIContent("Move Up"), false, () =>
                {
                    Undo.RegisterFullObjectHierarchyUndo(hotspot.transform.parent, "Move hotspot up");
                    hotspot.transform.SetSiblingIndex(index - 1);
                });
            }
            if (index == totalCount - 1)
            {
                menu.AddDisabledItem(new GUIContent("Move Down"));
            }
            else
            {
                menu.AddItem(new GUIContent("Move Down"), false , () =>
                {
                    Undo.RegisterFullObjectHierarchyUndo(hotspot.transform.parent, "Move hotspot down");
                    hotspot.transform.SetSiblingIndex(index + 1);
                });
            }
            menu.AddSeparator("");
            if (index == 0)
            {
                menu.AddDisabledItem(new GUIContent("Move to First"));
            }
            else
            {
                menu.AddItem(new GUIContent("Move to First"), false, () =>
                {
                    Undo.RegisterFullObjectHierarchyUndo(hotspot.transform.parent, "Move hotspot to first");
                    hotspot.transform.SetSiblingIndex(0);
                });
            }
            if (index == totalCount - 1)
            {
                menu.AddDisabledItem(new GUIContent("Move to Last"), false);
            }
            else
            {
                menu.AddItem(new GUIContent("Move to Last"), false, () =>
                {
                    Undo.RegisterFullObjectHierarchyUndo(hotspot.transform.parent, "Move hotspot to last");
                    hotspot.transform.SetSiblingIndex(totalCount - 1);
                });
            }
            menu.ShowAsContext();
        }

        private IEnumerator Export()
        {
            _isExporting = true;
            bool encounterException = false;
            var camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
            var originalCameraPos = camera.transform.position;
            var originalCameraRot = camera.transform.rotation;

            var count = _hotspots.Count(x => x.isActiveAndEnabled);

            var cubemap = RenderTexture.GetTemporary((int)_quality, (int)_quality, 0);
            cubemap.dimension = TextureDimension.Cube;

            var hotspotDataPath = Path.Combine(_outputFolder, "hotspots.json");

            var folderQualityName = $"{Enum.GetName(typeof(ImageQuality), _quality)}/";
            string qualityFolderPath = Path.Combine(_outputFolder, folderQualityName);

            bool encodeAsJPEG = _format == ImageFormat.JPEG ? true : false;

            Directory.CreateDirectory(qualityFolderPath);

            WorldData data = new WorldData
            {
                name = EditorSceneManager.GetActiveScene().name,
                quality = _quality,
                format = _format,
                loop = HotspotManager.Instance.loop
            };
            for (int index = 0; index < count; index++)
            {
                Hotspot hotspot = _hotspots[index];
                if (hotspot == null || !hotspot.isActiveAndEnabled)
                {
                    continue;
                }
                data.hotspots.Add(new HotspotData()
                {
                    guid = Guid.NewGuid().ToString(),
                    position = hotspot.transform.position,
                    interactions = hotspot.interactions
                });
                // Compute the filename
                var fileName = $"Panorama_{index}.{(encodeAsJPEG ? "jpeg" : "png")}";

                // Move the camera in the right position

                camera.transform.SetPositionAndRotation(hotspot.transform.position, Quaternion.identity);
                try
                {
                    EditorUtility.DisplayProgressBar("Exporting Panorama", $"Exporting {fileName}...", (float)index / count);
                    byte[] bytes = I360Render.Capture(ref cubemap, (int)_quality, encodeAsJPEG, camera);
                    if (bytes != null)
                    {
                        string path = Path.Combine(qualityFolderPath, fileName);
                        File.WriteAllBytes(path, bytes);
                    }
                }
                catch(Exception e)
                {
                    Debug.LogException(e);
                    encounterException = true;
                }
                if (encounterException)
                {
                    EditorUtility.ClearProgressBar();
                    RenderTexture.ReleaseTemporary(cubemap);
                    _isExporting = false;
                    yield return null;
                }
                yield return new WaitForEndOfFrame();
            }
            try
            {
                JsonSerializerSettings settings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    Converters = new JsonConverter[]
                    {
                    new Vector3Converter(),
                    new StringEnumConverter(),
                    new VersionConverter(),
                    new InteractionDataConverter()
                    }
                };
                string json = JsonConvert.SerializeObject(data, Formatting.Indented, settings);
                File.WriteAllText(hotspotDataPath, json);

                camera.transform.SetPositionAndRotation(originalCameraPos, originalCameraRot);
                if (_openFolderAfterExport)
                {
                    Application.OpenURL(_outputFolder);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                RenderTexture.ReleaseTemporary(cubemap);
                _isExporting = false;
            }
        }

        IEnumerator WaitUntilEndOfFrame()
        {
            yield return new WaitForEndOfFrame();
        }

        void IHasCustomMenu.AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(new GUIContent("About"), false, () =>
            {
                var window = GetWindow<AboutWindow>(true);
                window.titleContent = new GUIContent("About Treasured Toolkit for Unity");
                window.position = new Rect(200, 200, 300, 300);
                window.Show();
            });
        }

        private class AboutWindow : EditorWindow
        {
            private void OnGUI()
            {
                EditorGUILayout.LabelField(new GUIContent(Styles.TreasuredIcon), GUILayout.MinWidth(100), GUILayout.MinHeight(100));
                EditorGUILayout.LabelField(new GUIContent("Version"), new GUIContent("0.1.0"));
                if (EditorGUIUtils.Link(new GUIContent("Website"), new GUIContent("https://treasured.ca/")))
                {
                    Application.OpenURL("https://treasured.ca/");
                }
                if (EditorGUIUtils.Link(new GUIContent("Email"), new GUIContent("team@treasured.ca")))
                {
                    Application.OpenURL("mailto:team@treasured.ca");
                }
            }
        }
    }

    
}
