using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityGLTF;
using Object = UnityEngine.Object;

namespace Treasured.UnitySdk
{
    /// <summary>
    /// Combine and Export mesh
    /// </summary>
    [Serializable]
    [ExportProcessSettings(EnabledByDefault = false)]
    internal class MeshExportProcess : ExportProcess
    {
        private SerializedProperty _filterTag;
        private SerializedProperty _canUseTag;

        private SerializedProperty _filterLayerMask;
        private SerializedProperty _canUseLayerMask;

        private string _rootDirectory;

        [JsonIgnore]
        private bool _keepCombinedMeshInScene;

        private const string CombineMeshGameObject = "CombinedMesh";
        private string _terrainObjName => $"{SceneManager.GetActiveScene().name}_terrain";
        private string _resourcesFolder => Path.Combine(Application.dataPath, "Resources");
        private string _terrainObjSavePath => Path.Combine(_resourcesFolder, $"{_terrainObjName}.obj");
        
        private GameObject _terrainGameObject;
        
        private int _terrainCount;
        private int _progressCounter;
        private int _totalTerrainCount;
        private int _progressUpdateInterval = 10000;

        public override void OnEnable(SerializedObject serializedObject)
        {
            _filterTag = serializedObject.FindProperty(nameof(_filterTag));
            _canUseTag = serializedObject.FindProperty(nameof(_canUseTag));

            _filterLayerMask = serializedObject.FindProperty(nameof(_filterLayerMask));
            _canUseLayerMask = serializedObject.FindProperty(nameof(_canUseLayerMask));
        }

        public override void OnGUI(string root, SerializedObject serializedObject)
        {
            return;
            EditorGUILayout.BeginHorizontal();
            _canUseTag.boolValue = EditorGUILayout.Toggle(
                new GUIContent("Use Tag", "Only combine GameObjects which this tag."),
                _canUseTag.boolValue);

            if (_canUseTag.boolValue)
            {
                _filterTag.stringValue = EditorGUILayout.TagField("", _filterTag.stringValue);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            _canUseLayerMask.boolValue = EditorGUILayout.Toggle(
                new GUIContent("Use LayerMask",
                    "Only combine GameObjects which Layers is in this LayerMask."),
                _canUseLayerMask.boolValue);
            if (_canUseLayerMask.boolValue)
            {
                EditorGUILayout.PropertyField(_filterLayerMask, GUIContent.none);
            }

            EditorGUILayout.EndHorizontal();

            if (_canUseTag.boolValue || _canUseLayerMask.boolValue)
            {
                _keepCombinedMeshInScene = EditorGUILayout.Toggle(new GUIContent("Keep Combined Mesh",
                    "Keep combined mesh gameObject in scene after exporting to GLB mesh"), _keepCombinedMeshInScene);
            }
        }

        public override void OnExport(string rootDirectory, TreasuredMap map)
        {
            var mapCanUseTag = map.CanUseTag;
            var mapFilterTag = map.FilterTag;

            var mapCanUseLayerMask = map.CanUseLayerMask;
            var mapLayerMaskValue = map.FilterLayerMask.value;

            if (!mapCanUseTag && !mapCanUseLayerMask)
            {
                Debug.LogError("Mesh Export Search option is not configured. GLB Mesh will not be exported.");
                return;
            }

            _rootDirectory = rootDirectory;

            List<GameObject> meshToCombine = new List<GameObject>();

            //  Find terrain from the scene
            Terrain terrain = Terrain.activeTerrain;
            if (terrain)
            {
                //  check with the search filter
                if (mapCanUseTag)
                {
                    if (terrain.gameObject.CompareTag(mapFilterTag))
                    {
                        meshToCombine.Add(ExportTerrainToObj(terrain));
                    }
                }
                else if (mapCanUseLayerMask)
                {
                    var terrainLayer = 1 << terrain.gameObject.layer;
                    if ((mapLayerMaskValue & terrainLayer) == terrainLayer)
                    {
                        if (!meshToCombine.Contains(terrain.gameObject))
                        {
                            meshToCombine.Add(ExportTerrainToObj(terrain));
                        }
                    }
                }
            }

            var allGameObjectInScene = Object.FindObjectsOfType<GameObject>();

            foreach (var gameObject in allGameObjectInScene)
            {
                if (mapCanUseTag)
                {
                    //  Compare tag to see if needs to include in mesh combiner
                    if (gameObject.CompareTag(mapFilterTag))
                    {
                        meshToCombine.Add(gameObject);
                        continue;
                    }
                }

                if (mapCanUseLayerMask)
                {
                    var layer = 1 << gameObject.layer;
                    if ((mapLayerMaskValue & layer) == layer)
                    {
                        meshToCombine.Add(gameObject);
                        continue;
                    }
                }
            }

            //  Combining meshes
            CombineAllMeshes(meshToCombine, map.transform);
        }

        private void CombineAllMeshes(List<GameObject> meshToCombine, Transform parentTransform)
        {
            //  Checking meshToCombine for null
            if (meshToCombine.Count == 0)
            {
                Debug.LogError("No GameObjects were found based on the search filter. GLB mesh will not be exported.");
                return;
            }

            var tempGameObject = new GameObject(CombineMeshGameObject);
            var meshFilter = tempGameObject.AddComponent<MeshFilter>();
            var meshRenderer = tempGameObject.AddComponent<MeshRenderer>();
            var vertices = 0;

            var meshFilters = new List<MeshFilter>();

            foreach (var gameObject in meshToCombine)
            {
                if (ContainsValidRenderer(gameObject) && gameObject.TryGetComponent(out MeshFilter filter))
                {
                    meshFilters.Add(filter);

                    vertices += filter.sharedMesh.vertexCount;
                }
            }

            if (meshFilters.Count == 0)
            {
                Debug.LogError("No meshes found in the search filtered gameObjects. GLB mesh will not be exported.");
                return;
            }

            CombineInstance[] combine = new CombineInstance[meshFilters.Count];

            var i = 0;
            while (i < meshFilters.Count)
            {
                combine[i].mesh = meshFilters[i].sharedMesh;
                combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
                // meshFilters[i].gameObject.SetActive(false);

                i++;
            }

            var mesh = new Mesh();

            if (vertices > 65535)
            {
                Debug.Log("Using Index 32");
                mesh.indexFormat = IndexFormat.UInt32;
            }

            meshFilter.sharedMesh = mesh;
            meshFilter.sharedMesh.CombineMeshes(combine, true, true, false);
            meshRenderer.material =
                new Material(Resources.Load("TreasuredDefaultMaterial", typeof(Material)) as Material);
            tempGameObject.gameObject.SetActive(true);

            Transform[] exportTransforms = new Transform[2];
            exportTransforms[0] = tempGameObject.transform;
            exportTransforms[1] = parentTransform;
            CreateGLB(exportTransforms);

            if (!_keepCombinedMeshInScene)
            {
                Object.DestroyImmediate(tempGameObject);
                if (_terrainGameObject)
                {
                    Object.DestroyImmediate(_terrainGameObject);
                    FileUtil.DeleteFileOrDirectory(_terrainObjSavePath);
                    AssetDatabase.Refresh();
                }
            }
        }

        private void CreateGLB(Transform[] export)
        {
            var exportOptions = new ExportOptions { TexturePathRetriever = RetrieveTexturePath };
            var exporter = new GLTFSceneExporter(export, exportOptions);

            var name = SceneManager.GetActiveScene().name;

            if (!string.IsNullOrEmpty(_rootDirectory))
            {
                exporter.SaveGLB(_rootDirectory, name);
            }
        }

        private string RetrieveTexturePath(Texture texture)
        {
            return AssetDatabase.GetAssetPath(texture);
        }

        private bool ContainsValidRenderer(GameObject gameObject)
        {
            return (gameObject.GetComponent<MeshFilter>() != null && gameObject.GetComponent<MeshRenderer>() != null
                    || gameObject.GetComponent<SkinnedMeshRenderer>() != null)
                   && gameObject.GetComponent<TextMeshPro>() == null;
        }

        private GameObject ExportTerrainToObj(Terrain terrain)
        {
            if (!terrain)
            {
                Debug.LogError("No terrain found! Terrain will not be exported in Glb mesh");
                return new GameObject();
            }

            var terrainData = terrain.terrainData;
            var terrainPosition = terrain.transform.position;
            var terrainParsingFormat = TerrainParsingFormat.Triangles;
            var terrainExportQuality = TerrainExportQuality.Half;
            
            if (!Directory.Exists(_resourcesFolder))
            {
                Directory.CreateDirectory(_resourcesFolder);
            }
            
            var w = terrainData.heightmapResolution;
            var h = terrainData.heightmapResolution;
            var meshScale = terrainData.size;
            var tRes = (int)Mathf.Pow(2, (int)terrainExportQuality);
            meshScale = new Vector3(meshScale.x / (w - 1) * tRes, meshScale.y, meshScale.z / (h - 1) * tRes);
            var uvScale = new Vector2(1.0f / (w - 1), 1.0f / (h - 1));
            var tData = terrainData.GetHeights(0, 0, w, h);

            w = (w - 1) / tRes + 1;
            h = (h - 1) / tRes + 1;
            var tVertices = new Vector3[w * h];
            var tUV = new Vector2[w * h];

            int[] tPolys;

            if (terrainParsingFormat == TerrainParsingFormat.Triangles)
            {
                tPolys = new int[(w - 1) * (h - 1) * 6];
            }
            else
            {
                tPolys = new int[(w - 1) * (h - 1) * 4];
            }

            // Build vertices and UVs
            for (var y = 0; y < h; y++)
            {
                for (var x = 0; x < w; x++)
                {
                    tVertices[y * w + x] = Vector3.Scale(meshScale, new Vector3(-y, tData[x * tRes, y * tRes], x))
                                           + terrainPosition;
                    tUV[y * w + x] = Vector2.Scale(new Vector2(x * tRes, y * tRes), uvScale);
                }
            }

            var index = 0;
            if (terrainParsingFormat == TerrainParsingFormat.Triangles)
            {
                // Build triangle indices: 3 indices into vertex array for each triangle
                for (var y = 0; y < h - 1; y++)
                {
                    for (var x = 0; x < w - 1; x++)
                    {
                        // For each grid cell output two triangles
                        tPolys[index++] = (y * w) + x;
                        tPolys[index++] = ((y + 1) * w) + x;
                        tPolys[index++] = (y * w) + x + 1;

                        tPolys[index++] = ((y + 1) * w) + x;
                        tPolys[index++] = ((y + 1) * w) + x + 1;
                        tPolys[index++] = (y * w) + x + 1;
                    }
                }
            }
            else
            {
                // Build quad indices: 4 indices into vertex array for each quad
                for (var y = 0; y < h - 1; y++)
                {
                    for (var x = 0; x < w - 1; x++)
                    {
                        // For each grid cell output one quad
                        tPolys[index++] = (y * w) + x;
                        tPolys[index++] = ((y + 1) * w) + x;
                        tPolys[index++] = ((y + 1) * w) + x + 1;
                        tPolys[index++] = (y * w) + x + 1;
                    }
                }
            }

            // Export to .obj
            var sw = new StreamWriter(_terrainObjSavePath);
            try
            {
                sw.WriteLine("# Unity terrain OBJ File");

                // Write vertices
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
                _progressCounter = _terrainCount = 0;
                _totalTerrainCount =
                    (tVertices.Length * 2 + (terrainParsingFormat == TerrainParsingFormat.Triangles
                        ? tPolys.Length / 3
                        : tPolys.Length / 4)) / _progressUpdateInterval;
                for (var i = 0; i < tVertices.Length; i++)
                {
                    UpdateProgress();
                    var sb = new StringBuilder("v ", 20);
                    // StringBuilder stuff is done this way because it's faster than using the "{0} {1} {2}"etc. format
                    // Which is important when you're exporting huge terrains.
                    sb.Append(tVertices[i].x.ToString()).Append(" ").Append(tVertices[i].y.ToString()).Append(" ")
                        .Append(tVertices[i].z.ToString());
                    sw.WriteLine(sb);
                }

                // Write UVs
                for (var i = 0; i < tUV.Length; i++)
                {
                    UpdateProgress();
                    var sb = new StringBuilder("vt ", 22);
                    sb.Append(tUV[i].x.ToString()).Append(" ").Append(tUV[i].y.ToString());
                    sw.WriteLine(sb);
                }

                if (terrainParsingFormat == TerrainParsingFormat.Triangles)
                {
                    // Write triangles
                    for (var i = 0; i < tPolys.Length; i += 3)
                    {
                        UpdateProgress();
                        var sb = new StringBuilder("f ", 43);
                        sb.Append(tPolys[i] + 1).Append("/").Append(tPolys[i] + 1).Append(" ").Append(tPolys[i + 1] + 1)
                            .Append("/").Append(tPolys[i + 1] + 1).Append(" ").Append(tPolys[i + 2] + 1).Append("/")
                            .Append(tPolys[i + 2] + 1);
                        sw.WriteLine(sb);
                    }
                }
                else
                {
                    // Write quads
                    for (var i = 0; i < tPolys.Length; i += 4)
                    {
                        UpdateProgress();
                        var sb = new StringBuilder("f ", 57);
                        sb.Append(tPolys[i] + 1).Append("/").Append(tPolys[i] + 1).Append(" ").Append(tPolys[i + 1] + 1)
                            .Append("/").Append(tPolys[i + 1] + 1).Append(" ").Append(tPolys[i + 2] + 1).Append("/")
                            .Append(tPolys[i + 2] + 1).Append(" ").Append(tPolys[i + 3] + 1).Append("/")
                            .Append(tPolys[i + 3] + 1);
                        sw.WriteLine(sb);
                    }
                }
            }
            catch (Exception err)
            {
                Debug.Log($"Treasured Mesh Export: Error saving {_terrainObjName}.obj file: " + err.Message);
            }

            sw.Close();

            AssetDatabase.Refresh();

            EditorUtility.DisplayProgressBar($"Saving {_terrainObjName}.obj.", "Saving...", 1f);
            EditorUtility.ClearProgressBar();
            
            var terrainObj = Object.Instantiate(Resources.Load<GameObject>(_terrainObjName));
            _terrainGameObject = terrainObj;
            return terrainObj;
        }

        void UpdateProgress()
        {
            if (_progressCounter++ == _progressUpdateInterval)
            {
                _progressCounter = 0;
                EditorUtility.DisplayProgressBar($"Combining Terrain to {_terrainObjName}.obj...",
                    "This might take a while..", Mathf.InverseLerp(0, _totalTerrainCount, ++_terrainCount));
            }
        }

        //  Option to change the parsing format
        enum TerrainParsingFormat
        {
            Triangles,
            Quads
        }

        //  Option to change the export quality
        enum TerrainExportQuality
        {
            Full,
            Half,
            Quarter,
            Eighth,
            Sixteenth
        }
    }
}
