using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityGLTF;
using UnityMeshSimplifier;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Treasured.UnitySdk
{
    public class MeshExporter : Exporter
    {
        private const string CombineMeshGameObject = "CombinedMesh";
        private string _terrainObjName => $"{SceneManager.GetActiveScene().name}_terrain";
        private string _resourcesFolder => Path.Combine(Application.dataPath, "Resources");
        private string _terrainObjSavePath => Path.Combine(_resourcesFolder, $"{_terrainObjName}.obj");

        private GameObject _terrainGameObject;
        private int _terrainCount;
        private int _progressCounter;
        private int _totalTerrainCount;
        private int _progressUpdateInterval = 10000;

        public static string[] allTags;
        public int includeTags = 1;
        public int excludeTags = 0;
        public bool canUseTag;
        public int filterTag = 0;

        public bool canUseLayerMask;
        public LayerMask filterLayerMask;

        public MeshExportQuality ExportQuality = MeshExportQuality.Full;

        [Tooltip("Keep combined mesh gameObject in scene after exporting to GLB mesh")]
        public bool keepCombinedMesh;

        [Tooltip("Display detailed mesh exporter logs")]
        public bool displayLogs;

        [UnityEngine.ContextMenu("Reset")]
        private void Reset()
        {
            enabled = true;
            includeTags = 1;
            excludeTags = 0;
            canUseTag = false;
            filterTag = 0;
            canUseLayerMask = false;
            filterLayerMask = 0;
            ExportQuality = MeshExportQuality.Full;
        }

        public override void OnPreExport()
        {
            if (!canUseTag && !canUseLayerMask)
            {
                throw new ArgumentException(
                    "[MeshExporter] : Mesh Export Search option is not configured.GLB Mesh will not be exported.");
            }

            if (canUseTag)
            {
                //  Check if include and exclude tags does not have common tags
                if ((includeTags & excludeTags) != 0)
                {
                    throw new ArgumentException(
                        "[MeshExporter] : Mesh Export tag for Include Tags and Exclude Tags have common tags assigned. "
                        + "Please make sure that same tag is not selected on both.");
                }
            }
        }

        public override void Export()
        {
            if (!canUseTag && !canUseLayerMask)
            {
                Debug.LogError(
                    "[MeshExporter] : Mesh Export Search option is not configured. GLB Mesh will not be exported.");
                return;
            }

            var meshToCombineDictionary = PrepareMeshForExport();

            //  Combining meshes
            CombineAllMeshes(meshToCombineDictionary.Values.ToList(), Map.transform);
        }

        public Dictionary<int, GameObject> PrepareMeshForExport()
        {
            Dictionary<int, GameObject> meshToCombineDictionary = new Dictionary<int, GameObject>();
            filterTag = includeTags ^ excludeTags;

            //  Find terrain from the scene
            Terrain terrain = Terrain.activeTerrain;
            if (terrain)
            {
                //  check with the search filter
                if (canUseTag)
                {
                    meshToCombineDictionary.Add(terrain.GetInstanceID(), terrain.gameObject);

                    var terrainTagIndex = (int)Mathf.Pow(2, Array.IndexOf(allTags, terrain.gameObject.tag));
                    if ((includeTags & terrainTagIndex) == terrainTagIndex)
                    {
                        var exportTerrainToObj = ExportTerrainToObj(terrain);
                        meshToCombineDictionary.Add(exportTerrainToObj.GetInstanceID(), exportTerrainToObj);
                    }
                }
                else if (canUseLayerMask)
                {
                    var terrainLayer = 1 << terrain.gameObject.layer;
                    if ((filterLayerMask & terrainLayer) == terrainLayer)
                    {
                        if (!meshToCombineDictionary.ContainsKey(terrain.GetInstanceID()))
                        {
                            var exportTerrainToObj = ExportTerrainToObj(terrain);
                            meshToCombineDictionary.Add(exportTerrainToObj.GetInstanceID(), exportTerrainToObj);
                        }
                    }
                }
            }

            var rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();

            foreach (var gameObject in rootGameObjects)
            {
                //  If gameObject is disabled then skip adding it to export
                if (!gameObject.activeInHierarchy)
                {
                    continue;
                }
                
                if (meshToCombineDictionary.ContainsKey(gameObject.GetInstanceID()))
                {
                    if (displayLogs)
                    {
                        Debug.Log(
                            $"[MeshExporter] : {gameObject.name} is already included in combined mesh dictionary. Duplicate will be ignored.");
                    }

                    continue;
                }

                if (canUseTag)
                {
                    //  Compare tag to see if needs to include in mesh combiner
                    var gameObjectTagIndex = (int)Mathf.Pow(2, Array.IndexOf(allTags, gameObject.tag));
                    if ((includeTags & gameObjectTagIndex) == gameObjectTagIndex)
                    {
                        foreach (var child in gameObject.transform.GetComponentsInChildren<Transform>())
                        {
                            if (!child.gameObject.activeInHierarchy)
                            {
                                continue;
                            }

                            var childGameObjectTagIndex = (int)Mathf.Pow(2, Array.IndexOf(allTags, child.tag));
                            if ((excludeTags & childGameObjectTagIndex) != childGameObjectTagIndex)
                            {
                                meshToCombineDictionary.Add(child.gameObject.GetInstanceID(), child.gameObject);
                            }
                        }

                        continue;
                    }
                }

                if (canUseLayerMask)
                {
                    var layer = 1 << gameObject.layer;
                    if ((filterLayerMask & layer) == layer)
                    {
                        foreach (var child in gameObject.transform.GetComponentsInChildren<Transform>())
                        {
                            if (!child.gameObject.activeInHierarchy)
                            {
                                continue;
                            }
                            
                            meshToCombineDictionary.Add(child.gameObject.GetInstanceID(), child.gameObject);
                        }

                        continue;
                    }
                }
            }

            /*
             * Check for Lod group
             * Remove more triangle lod gameObjects
             */
            var lodFilterCheckDictionary = new Dictionary<int, GameObject>(meshToCombineDictionary);
            foreach (var dict in lodFilterCheckDictionary)
            {
                //  Get the gameObjects which has different LOD group meshes
                if (dict.Value.TryGetComponent(out LODGroup lodGroup))
                {
                    var lodCount = lodGroup.lodCount;
                    var allLodGroups = lodGroup.GetLODs();
                    if (lodCount == 0)
                    {
                        continue;
                    }

                    //  TODO: Compare triangles and only include least triangles count

                    //  Check if length of renderers are not 0
                    if (allLodGroups[lodCount - 1].renderers.Length > 0 && lodCount - 2 > 0)
                    {
                        //  Remove all the other lodGroup's gameObjects from meshToCombine
                        for (var i = lodCount - 2; i >= 0; i--)
                        {
                            var renderers = allLodGroups[i].renderers;
                            if (renderers.Length != 0)
                            {
                                foreach (var renderer in renderers)
                                {
                                    if (meshToCombineDictionary.ContainsKey(renderer.gameObject.GetInstanceID()))
                                    {
                                        if (displayLogs)
                                        {
                                            Debug.Log(
                                                $"[MeshExporter] : Excluded LOD - {meshToCombineDictionary[renderer.gameObject.GetInstanceID()]?.name}",
                                                renderer.gameObject);
                                        }

                                        meshToCombineDictionary.Remove(renderer.gameObject.GetInstanceID());
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return meshToCombineDictionary;
        }

        private void CombineAllMeshes(List<GameObject> meshToCombine, Transform parentTransform)
        {
            //  Checking meshToCombine for null
            if (meshToCombine.Count == 0)
            {
                Debug.LogError(
                    "[MeshExporter] : No GameObjects were found based on the search filter. GLB mesh will not be exported.");
                return;
            }

            var verticesCount = 0;
            var verticesList = new List<Vector3>();
            var meshTriangles = new List<int>();
            var meshQuality = (int)ExportQuality;
            var simplificationOption = new SimplificationOptions
            {
                PreserveBorderEdges = true,
                PreserveUVSeamEdges = false,
                PreserveUVFoldoverEdges = false,
                PreserveSurfaceCurvature = false,
                EnableSmartLink = true,
                VertexLinkDistance = Double.Epsilon,
                MaxIterationCount = 100,
                Agressiveness = 7.0,
                ManualUVComponentCount = false,
                UVComponentCount = 0
            };

            foreach (var meshGameObject in meshToCombine)
            {
                if (ContainsValidRenderer(meshGameObject) && meshGameObject.TryGetComponent(out MeshFilter filter))
                {
                    //  Check if sharedMesh is not null
                    if (filter.sharedMesh != null)
                    {
                        var optimizedMesh = filter.sharedMesh;

                        if (meshQuality != 0 && filter.sharedMesh.triangles.Length > meshQuality)
                        {
                            if (displayLogs)
                            {
                                Debug.Log(
                                    $"[MeshExporter] : {filter.gameObject.name} GameObject has {filter.sharedMesh.triangles.Length} vertices which exceeds the vertices limit {meshQuality}. Optimizing Mesh...",
                                    filter.gameObject);
                            }

                            var meshSimplifier = new MeshSimplifier(filter.sharedMesh);

                            meshSimplifier.SimplificationOptions = simplificationOption;
                            meshSimplifier.SimplifyMesh(0.1f);

                            optimizedMesh = meshSimplifier.ToMesh();
                        }

                        //  Adding mesh vertices and triangles
                        foreach (var meshVertex in optimizedMesh.vertices)
                        {
                            verticesList.Add(meshGameObject.transform.TransformPoint(meshVertex));
                        }

                        for (var j = 0; j < optimizedMesh.subMeshCount; j++)
                        {
                            var triangles = optimizedMesh.GetTriangles(j);
                            foreach (var t in triangles)
                            {
                                meshTriangles.Add(verticesCount + t);
                            }
                        }

                        verticesCount = verticesList.Count;
                    }
                }
            }

            if (verticesCount == 0)
            {
                Debug.LogError(
                    "[MeshExporter] : No Valid mesh were found based on the search filter. GLB mesh will not be exported.");
                return;
            }

            var mesh = new Mesh();

            if (verticesCount > 65535)
            {
                mesh.indexFormat = IndexFormat.UInt32;
            }

            var tempGameObject = new GameObject(CombineMeshGameObject);
            var meshFilter = tempGameObject.AddComponent<MeshFilter>();
            var meshRenderer = tempGameObject.AddComponent<MeshRenderer>();

            //  Optimizations
            List<Vector3> newVertices = new List<Vector3>();
            Matrix4x4 mt = tempGameObject.transform.localToWorldMatrix;

            for (int i = 0; i < verticesList.Count; i++)
            {
                newVertices.Add(mt.MultiplyPoint3x4(verticesList[i]));
            }

            mesh.vertices = newVertices.ToArray();
            mesh.SetTriangles(meshTriangles, 0);

            meshFilter.mesh = mesh;
            meshRenderer.material =
                new Material(Resources.Load("TreasuredDefaultMaterial", typeof(Material)) as Material);
            tempGameObject.gameObject.SetActive(true);

            var exportTransforms = new Transform[2];
            exportTransforms[0] = tempGameObject.transform;
            exportTransforms[1] = parentTransform;
            CreateGLB(exportTransforms);

            if (!keepCombinedMesh)
            {
                Object.DestroyImmediate(tempGameObject);
                if (_terrainGameObject)
                {
                    Object.DestroyImmediate(_terrainGameObject);
#if UNITY_EDITOR
                    FileUtil.DeleteFileOrDirectory(_terrainObjSavePath);
                    AssetDatabase.Refresh();
#endif
                }
            }
        }

        private void CreateGLB(Transform[] export)
        {
            var exportOptions = new ExportOptions { TexturePathRetriever = RetrieveTexturePath };
            var exporter = new GLTFSceneExporter(export, exportOptions);

            if (!string.IsNullOrEmpty(Map.exportSettings.OutputDirectory))
            {
                exporter.SaveGLB(Map.exportSettings.OutputDirectory.ToOSSpecificPath(), "scene");
            }
        }

        private string RetrieveTexturePath(Texture texture)
        {
#if UNITY_EDITOR
            return AssetDatabase.GetAssetPath(texture);
#else
            return texture.name;
#endif
        }

        private bool ContainsValidRenderer(GameObject gameObject)
        {
            return gameObject.GetComponent<ReflectionProbe>() == null
                   && (gameObject.GetComponent<MeshFilter>() != null && gameObject.GetComponent<MeshRenderer>() != null
                       || gameObject.GetComponent<SkinnedMeshRenderer>() != null)
#if TEXTMESHPRO_3_0_6_OR_NEWER
                   && gameObject.GetComponent<TMPro.TextMeshPro>() == null;
#endif
        }

        private GameObject ExportTerrainToObj(Terrain terrain)
        {
            if (!terrain)
            {
                Debug.LogError("[MeshExporter] : No terrain found! Terrain will not be exported in Glb mesh");
                return new GameObject();
            }

            var terrainData = terrain.terrainData;
            var terrainPosition = terrain.transform.position;
            var terrainParsingFormat = TerrainParsingFormat.Triangles;
            var terrainExportQuality = 1; // Full: 0, Half: 1, Quarter: 2, Eighth: 3, Sixteenth: 4

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
            var sw = new StreamWriter(_terrainObjSavePath.ToOSSpecificPath());
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
                Debug.LogError($"[MeshExporter] : Error saving {_terrainObjName}.obj file: " + err.Message);
            }

            sw.Close();
#if UNITY_EDITOR
            AssetDatabase.Refresh();

            EditorUtility.DisplayProgressBar($"Saving {_terrainObjName}.obj.", "Saving...", 1f);
            EditorUtility.ClearProgressBar();
#endif
            var terrainObj = Object.Instantiate(Resources.Load<GameObject>(_terrainObjName));
            _terrainGameObject = terrainObj;
            return terrainObj;
        }

        void UpdateProgress()
        {
#if UNITY_EDITOR
            if (_progressCounter++ == _progressUpdateInterval)
            {
                _progressCounter = 0;
                EditorUtility.DisplayProgressBar($"Combining Terrain to {_terrainObjName}.obj...",
                    "This might take a while..", Mathf.InverseLerp(0, _totalTerrainCount, ++_terrainCount));
            }
#endif
        }

        //  Option to change the parsing format
        enum TerrainParsingFormat
        {
            Triangles,
            Quads
        }
    }
}
