using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    internal static class ScriptableObjectExtensionMethods
    {
        public static void RemoveAllSubAssets(this ScriptableObject scriptableObject)
        {
            if (EditorUtility.IsPersistent(scriptableObject))
            {
                string assetPath = AssetDatabase.GetAssetPath(scriptableObject);
                foreach (var subAsset in AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath))
                {
                    AssetDatabase.RemoveObjectFromAsset(subAsset);
                }
                AssetDatabase.SaveAssets();
            }
        }
    }
}
