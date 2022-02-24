using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    /// <summary>
    /// Class to register GameObject menu items.
    /// </summary>
    internal static class MenuItemRegister
    {
        static bool IsTreasuredMapSelected()
        {
            return Selection.activeGameObject?.GetComponentInParent<TreasuredMap>();
        }

        /// <summary>
        /// Creates a new object of type <typeparamref name="T"/> and add it to the selected TreasuredMap under a game object with <paramref name="categoryName"/>.
        /// Create new object if the map or category object not found.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="categoryName"></param>
        static void CreateNew<T>(string categoryName) where T : TreasuredObject
        {
            TreasuredMap map = Selection.activeGameObject.GetComponentInParent<TreasuredMap>();
            Transform mapTransform = map.transform;
            Transform categoryRoot = mapTransform.Find(categoryName);
            if (categoryRoot == null)
            {
                categoryRoot = new GameObject(categoryName).transform;
                categoryRoot.SetParent(mapTransform);
            }
            string uniqueName = ObjectNames.GetUniqueName(Enumerable.Range(0, categoryRoot.childCount).Select(index => categoryRoot.GetChild(index).name).ToArray(), ObjectNames.NicifyVariableName(typeof(T).Name));
            GameObject newObject = new GameObject(uniqueName, typeof(T));
            newObject.transform.SetParent(categoryRoot);
#if UNITY_2020_3_OR_NEWER
            // Enable renaming mode
            Selection.activeGameObject = newObject;
            var type = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
            var hierarchyWindow = EditorWindow.GetWindow(type);
            var rename = type.GetMethod("FrameAndRenameNewGameObject", BindingFlags.Static | BindingFlags.NonPublic);
            rename.Invoke(null, null);
#endif
        }

        [MenuItem("GameObject/Treasured/Sound Source", false, 49)]
        static void CreateSoundSource()
        {
            CreateNew<SoundSource>("Sounds");
        }

        [MenuItem("GameObject/Treasured/Sound Source", true, 49)]
        static bool CanCreateSoundSource()
        {
            return IsTreasuredMapSelected();
        }

        [MenuItem("GameObject/Treasured/Hotspot", false, 49)]
        static void CreateHotspot()
        {
            CreateNew<Hotspot>("Hotspots");
        }

        [MenuItem("GameObject/Treasured/Hotspot", true, 49)]
        static bool CanCreateHotspot()
        {
            return IsTreasuredMapSelected();
        }

        [MenuItem("GameObject/Treasured/Interactable", false, 49)]
        static void CreateInteractable()
        {
            CreateNew<Interactable>("Interactables");
        }

        [MenuItem("GameObject/Treasured/Interactable", true, 49)]
        static bool CanCreateInteractable()
        {
            return IsTreasuredMapSelected();
        }

        [MenuItem("GameObject/Treasured/Video Renderer", false, 49)]
        static void CreateVideoRenderer()
        {
            CreateNew<VideoRenderer>("Videos");
        }

        [MenuItem("GameObject/Treasured/Video Renderer", true, 49)]
        static bool CanCreateVideoRenderer()
        {
            return IsTreasuredMapSelected();
        }

        [MenuItem("GameObject/Treasured/Empty Map", false, 49)]
        static void CreateEmptyMap()
        {
            GameObject map = new GameObject("Treasured Map", typeof(TreasuredMap));
            if (Selection.activeGameObject)
            {
                map.transform.SetParent(Selection.activeGameObject.transform);
            }
        }

        [MenuItem("GameObject/Treasured/Empty Map", true, 49)]
        static bool CanCreateEmptyMap()
        {
            if (Selection.activeGameObject == null)
            {
                return true;
            }
            return !Selection.activeGameObject.GetComponentInParent<TreasuredMap>();
        }
    }
}
