using Newtonsoft.Json;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk.Editor
{
    internal partial class TreasuredMapEditor
    {
        static T CreateTreasuredObject<T>(Transform parent) where T : TreasuredObject
        {
            GameObject go = new GameObject();
            go.transform.SetParent(parent);
            return go.AddComponent<T>();
        }

        [MenuItem("GameObject/Treasured/Create Interactable", false, 49)]
        static void CreateInteractableFromContextMenu()
        {
            TreasuredMap map = Selection.activeGameObject.GetComponentInParent<TreasuredMap>();
            Transform root = map.transform;
            Transform interactableRoot = root.Find("Interactables");
            if (interactableRoot == null)
            {
                interactableRoot = new GameObject("Interactables").transform;
                interactableRoot.SetParent(root);
            }
            GameObject interactable = new GameObject("New Interacatble", typeof(Interactable));
            if (Selection.activeGameObject.transform == root)
            {
                interactable.transform.SetParent(interactableRoot);
            }
            else
            {
                interactable.transform.SetParent(Selection.activeGameObject.transform);
            }
        }

        [MenuItem("GameObject/Treasured/Create Hotspot", false, 49)]
        static void CreateHotspotFromContextMenu()
        {
            TreasuredMap map = Selection.activeGameObject.GetComponentInParent<TreasuredMap>();
            Transform root = map.transform;
            Transform hotspotRoot = root.Find("Hotspots");
            if (hotspotRoot == null)
            {
                hotspotRoot = new GameObject("Hotspots").transform;
                hotspotRoot.SetParent(root);
            }
            GameObject hotspot = new GameObject("New Hotspot", typeof(Hotspot));
            if (Selection.activeGameObject.transform == root)
            {
                hotspot.transform.SetParent(hotspotRoot);
            }
            else
            {
                hotspot.transform.SetParent(Selection.activeGameObject.transform);
            }
        }

        [MenuItem("GameObject/Treasured/Create Empty Map", false, 49)]
        static void CreateEmptyMap()
        {
            GameObject map = new GameObject("Treasured Map", typeof(TreasuredMap));
            if (Selection.activeGameObject)
            {
                map.transform.SetParent(Selection.activeGameObject.transform);
            }
        }

        [MenuItem("GameObject/Treasured/Create Empty Map", true, 49)]
        static bool CanCreateEmptyMap()
        {
            if(Selection.activeGameObject == null)
            {
                return true;
            }
            return !Selection.activeGameObject.GetComponentInParent<TreasuredMap>();
        }

        [MenuItem("GameObject/Treasured/Create Map from Json", false, 49)]
        static void CreateMapFromJson()
        {
            string jsonPath = EditorUtility.OpenFilePanel("Select Json", Utility.ProjectPath, "json");
            if (!File.Exists(jsonPath))
            {
                return;
            }
            string json = File.ReadAllText(jsonPath);
            try
            {
                TreasuredMapData data = JsonConvert.DeserializeObject<TreasuredMapData>(json);
                GameObject mapGO = new GameObject("Treasured Map");
                TreasuredMap map = mapGO.AddComponent<TreasuredMap>();
                map.Populate(data);

                GameObject hotspotRoot = new GameObject("Hotspots");
                hotspotRoot.transform.SetParent(mapGO.transform);

                GameObject interactableRoot = new GameObject("Interactables");
                interactableRoot.transform.SetParent(mapGO.transform);

                for (int i = 0; i < data.Hotspots.Count; i++)
                {
                    CreateTreasuredObject<Hotspot>(hotspotRoot.transform).BindData(data.Hotspots[i]);
                }

                for (int i = 0; i < data.Interactables.Count; i++)
                {
                    CreateTreasuredObject<Interactable>(interactableRoot.transform).BindData(data.Interactables[i]);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.StackTrace);
                throw e;
            }
        }

        [MenuItem("GameObject/Treasured/Create Map from Json", true, 49)]
        static bool CanCreateMapFromJson()
        {
            return Selection.activeGameObject == null;
        }

        [MenuItem("GameObject/Treasured/Create Hotspot", true)]
        static bool CanCreateHotspotFromContextMenu()
        {
            return Selection.activeGameObject?.GetComponentInParent<TreasuredMap>();
        }

        [MenuItem("GameObject/Treasured/Create Interactable", true, 49)]
        static bool CanCreateInteractableFromContextMenu()
        {
            return Selection.activeGameObject?.GetComponentInParent<TreasuredMap>();
        }
    }
}
