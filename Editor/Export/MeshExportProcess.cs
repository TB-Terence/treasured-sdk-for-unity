using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityGLTF;
using UnityGLTF.Extensions;

namespace Treasured.UnitySdk
{
    internal class MeshExportProcess : ExportProcess
    {
        private SerializedProperty _tag;
        private SerializedProperty _useTag;
        
        private SerializedProperty _layerMask;
        private SerializedProperty _useLayerMask;
        
        public override void OnEnable(SerializedObject serializedObject)
        {
            _tag = serializedObject.FindProperty(nameof(_tag));
            _useTag = serializedObject.FindProperty(nameof(_useTag));
            
            _layerMask = serializedObject.FindProperty(nameof(_layerMask));
            _useLayerMask = serializedObject.FindProperty(nameof(_useLayerMask));
        }

        public override void OnGUI(SerializedObject serializedObject)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(new GUIContent("Use Tag", "Only combine GameObjects which this tag."));
            EditorGUILayout.PropertyField(_useTag, GUIContent.none, GUILayout.Width(25));
            if (_useTag.boolValue)
            {
                _tag.stringValue = EditorGUILayout.TagField("", _tag.stringValue);
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(new GUIContent("Use LayerMask", "Only combine GameObjects which Layer is in this LayerMask."));
            EditorGUILayout.PropertyField(_useLayerMask, GUIContent.none, GUILayout.Width(25));
            if (_useLayerMask.boolValue)
            {
                EditorGUILayout.PropertyField(_layerMask, GUIContent.none);
            }
            EditorGUILayout.EndHorizontal();
        }

        public override void OnExport(string rootDirectory, TreasuredMap map)
        {
            Vector3 originalVector3 = new Vector3(0.5f, 0.5f, 0.5f);

            Debug.Log(SchemaExtensions.ToGltfVector3Convert(originalVector3).X);
            Debug.Log(SchemaExtensions.ToGltfVector3Convert(originalVector3).Y);
            Debug.Log(SchemaExtensions.ToGltfVector3Convert(originalVector3).Z);

            if (!map._useTag && !map._useLayerMask)
            {
                Debug.Log("Mesh not exported.");
                return;    
            }
            
            List<GameObject> meshToCombine = new List<GameObject>();

            var allGameObjectInScene = Object.FindObjectsOfType<GameObject>();

            foreach (var gameObject in allGameObjectInScene)
            {
                if (map._useTag)
                {
                    Debug.Log("Using Tags");
                    //  Compare tag to see if needs to include in mesh combiner
                    if (gameObject.CompareTag(map._tag))
                    {
                        meshToCombine.Add(gameObject);
                        continue;
                    }
                }

                if (map._useLayerMask)
                {
                    Debug.Log("Using LayerMask");
                    var layer = 1 << gameObject.layer;
                    if ((map._layerMask.value & layer) == layer)
                    {
                        meshToCombine.Add(gameObject);
                        continue;
                    }
                }
            }

            Debug.Log($"found GameObjects: {meshToCombine.Count}");
            // CombineAllMeshes(meshToCombine, map.transform);
        }
        
        private void CombineAllMeshes(List<GameObject> meshToCombine, Transform parentTransform)
        {
            //  Check meshToCombine for null

            GameObject tempGameObject = new GameObject("Combined Meshes");
            var meshFilter = tempGameObject.AddComponent<MeshFilter>();
            var meshRenderer = tempGameObject.AddComponent<MeshRenderer>();
            var vertices = 0;

            List<MeshFilter> meshFilters = new List<MeshFilter>();
            
            foreach (var gameObject in meshToCombine)
            {
                if (ContainsValidRenderer(gameObject) && gameObject.TryGetComponent(out MeshFilter filter))
                {
                    meshFilters.Add(filter);

                    vertices += filter.sharedMesh.vertexCount;
                }
            }

            CombineInstance[] combine = new CombineInstance[meshFilters.Count];

            int i = 0;
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
            meshRenderer.material = new Material(Resources.Load("TreasuredDefaultMaterial", typeof(Material)) as Material);
            tempGameObject.gameObject.SetActive(true);

            Transform[] exportTransforms = new Transform[2];
            exportTransforms[0] = tempGameObject.transform;
            exportTransforms[1] = parentTransform;
            CreateGLB(exportTransforms);
        }

        private void CreateGLB(Transform[] export)
        {
            var exportOptions = new ExportOptions { TexturePathRetriever = RetrieveTexturePath };
            var exporter = new GLTFSceneExporter(export, exportOptions);

            var path = EditorUtility.OpenFolderPanel("GLB Export Path", "", "");
            var name = SceneManager.GetActiveScene().name;
            
            if (!string.IsNullOrEmpty(path))
            {
                exporter.SaveGLB(path, name);
            }
        }

        private string RetrieveTexturePath(Texture texture)
        {
            return AssetDatabase.GetAssetPath(texture);
        }
        
        private bool ContainsValidRenderer (GameObject gameObject)
        {
            return ((gameObject.GetComponent<MeshFilter>() != null && gameObject.GetComponent<MeshRenderer>() != null) 
                    || (gameObject.GetComponent<SkinnedMeshRenderer>() != null)) && gameObject.GetComponent<TextMeshPro>() == null;
        }

    }
}
