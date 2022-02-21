using System.Collections.Generic;
using Newtonsoft.Json;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityGLTF;

namespace Treasured.UnitySdk
{
    /// <summary>
    /// Combine and Export mesh
    /// </summary>
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

        public override void OnEnable(SerializedObject serializedObject)
        {
            _filterTag = serializedObject.FindProperty(nameof(_filterTag));
            _canUseTag = serializedObject.FindProperty(nameof(_canUseTag));

            _filterLayerMask = serializedObject.FindProperty(nameof(_filterLayerMask));
            _canUseLayerMask = serializedObject.FindProperty(nameof(_canUseLayerMask));
        }

        public override void OnGUI(SerializedObject serializedObject)
        {
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
                    "Only combine GameObjects which Layer is in this LayerMask."),
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
            if (!map.CanUseTag && !map.CanUseLayerMask)
            {
                Debug.LogError("Mesh Export Search option is not configured. GLB Mesh will not be exported.");
                return;
            }

            _rootDirectory = rootDirectory;

            List<GameObject> meshToCombine = new List<GameObject>();

            var allGameObjectInScene = Object.FindObjectsOfType<GameObject>();

            foreach (var gameObject in allGameObjectInScene)
            {
                if (map.CanUseTag)
                {
                    //  Compare tag to see if needs to include in mesh combiner
                    if (gameObject.CompareTag(map.FilterTag))
                    {
                        meshToCombine.Add(gameObject);
                        continue;
                    }
                }

                if (map.CanUseLayerMask)
                {
                    var layer = 1 << gameObject.layer;
                    if ((map.FilterLayerMask.value & layer) == layer)
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
    }
}
