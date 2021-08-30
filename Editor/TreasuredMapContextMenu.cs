using Newtonsoft.Json;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk.Editor
{
    internal partial class TreasuredMapEditor
    {

        [MenuItem("GameObject/Treasured/Create Map from Json", false)]
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
                    CreateTreasuredObject<Hotspot>(hotspotRoot.transform,data.Hotspots[i]);
                }

                for (int i = 0; i < data.Interactables.Count; i++)
                {
                    CreateTreasuredObject<Interactable>(interactableRoot.transform, data.Interactables[i]);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.StackTrace);
                throw e;
            }
        }

        static void CreateTreasuredObject<T>(Transform parent, TreasuredObjectData data) where T : TreasuredObject
        {
            GameObject go = new GameObject(data.Name);
            go.transform.SetParent(parent);
            T obj = go.AddComponent<T>();
            obj.LoadFromData(data);
        }

        [MenuItem("GameObject/Treasured/Create Hotspot", false)]
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

        [MenuItem("GameObject/Treasured/Create Interactable", false)]
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

        [MenuItem("GameObject/Treasured/Create Map from Json", true)]
        static bool CanCreateMapFromJson()
        {
            return Selection.activeGameObject == null;
        }

        [MenuItem("GameObject/Treasured/Create Hotspot", true)]
        static bool CanCreateHotspotFromContextMenu()
        {
            return Selection.activeGameObject?.GetComponentInParent<TreasuredMap>();
        }

        [MenuItem("GameObject/Treasured/Create Interactable", true)]
        static bool CanCreateInteractableFromContextMenu()
        {
            return Selection.activeGameObject?.GetComponentInParent<TreasuredMap>();
        }
    }
}
