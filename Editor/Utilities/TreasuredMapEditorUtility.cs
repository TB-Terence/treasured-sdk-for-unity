using System;
using System.Collections.Generic;
using UnityEditor;

namespace Treasured.UnitySdk.Editor
{
    internal static class TreasuredMapEditorUtility
    {
        private static Dictionary<string, Dictionary<string, string>> s_ids = new Dictionary<string, Dictionary<string, string>>();

        public static void UpdateId(SerializedProperty treasuredObject, string oldId, string newId)
        {
            if (!(treasuredObject.serializedObject.targetObject is TreasuredObject obj))
            {
                return;
            }
            TreasuredMap map = GetMap(treasuredObject);
            if (map)
            {
                if (!s_ids.ContainsKey(map.Data.Id))
                {
                    s_ids[map.Data.Id] = new Dictionary<string, string>();
                }
                s_ids[map.Data.Id].Remove(oldId);
                s_ids[map.Data.Id][newId] = obj.GetRelativePath<TreasuredMap>();
            }
        }

        public static void RefreshIds(TreasuredMap map)
        {
            if (!map)
            {
                return;
            }
            if (!s_ids.ContainsKey(map.Data.Id))
            {
                s_ids[map.Data.Id] = new Dictionary<string, string>();
            }
            else
            {
                s_ids[map.Data.Id].Clear();
            }
            foreach (var hotspot in map.GetComponentsInChildren<Hotspot>())
            {
                s_ids[map.Data.Id][hotspot.Data.Id] = hotspot.GetRelativePath<TreasuredMap>();
            }
            foreach (var interactable in map.GetComponentsInChildren<Interactable>())
            {
                s_ids[map.Data.Id][interactable.Data.Id] = interactable.GetRelativePath<TreasuredMap>();
            }
        }

        public static void RefreshIds(SerializedProperty property)
        {
            if (!(property.serializedObject.targetObject is TreasuredObject obj))
            {
                return;
            }
            RefreshIds(obj.gameObject.GetComponentInParent<TreasuredMap>());
        }

        public static IEnumerable<KeyValuePair<string, string>> GetPathsForMap(SerializedProperty treasuredObject)
        {
            TreasuredMap map = GetMap(treasuredObject);
            if (!map)
            {
                throw new ArgumentException($"Map not found for {treasuredObject}.");
            }
            if (map.Data == null)
            {
                throw new ArgumentException($"Map data not found for {treasuredObject}.");
            }
            return s_ids[map.Data.Id];
        }

        public static string GetRelativePath(SerializedProperty property, string id)
        {
            TreasuredMap map = GetMap(property);
            if (map)
            {
                if (!s_ids.ContainsKey(map.Data.Id))
                {
                    RefreshIds(map);
                }
                return s_ids[map.Data.Id][id];
            }
            return string.Empty;
        }

        private static TreasuredMap GetMap(SerializedProperty treasuredObject)
        {
            if (treasuredObject.serializedObject.targetObject is TreasuredObject obj)
            {
                return obj.gameObject.GetComponentInParent<TreasuredMap>();
            }
            return null;
        }
    }
}
