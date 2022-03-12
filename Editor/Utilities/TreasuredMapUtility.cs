using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    internal static class TreasuredMapUtility
    {
        /// <summary>
        /// Creates a new object of type <typeparamref name="T"/> and add it to the TreasuredMap under a game object with <paramref name="categoryName"/>.
        /// Create new object if the category root not found. Place the new game object on floor if collider found otherwise place it at (0, 0, 0)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="map">The parent of the new object.</param>
        /// <returns>Treasured Object of Type <typeparamref name="T"/></returns>
        public static T CreateObject<T>(this TreasuredMap map) where T : TreasuredObject
        {
            string categoryName = ObjectNames.NicifyVariableName(typeof(T).Name + "s");
            Transform categoryRoot = map.transform.Find(categoryName);
            if (categoryRoot == null)
            {
                categoryRoot = new GameObject(categoryName).transform;
                categoryRoot.SetParent(map.transform);
            }
            string uniqueName = GameObjectUtility.GetUniqueNameForSibling(categoryRoot, ObjectNames.NicifyVariableName(typeof(T).Name));
            GameObject newGO = new GameObject(uniqueName);
            T obj = newGO.AddComponent<T>();
            newGO.transform.SetParent(categoryRoot);
            obj.TryInvokeMethods("OnSelectedInHierarchy");
            // Place the new game object on floor if collider found.
            Camera camera = SceneView.lastActiveSceneView.camera;
            if (Physics.Raycast(camera.transform.position, camera.transform.forward, out var hit))
            {
                obj.transform.position = hit.point;
                if (obj is Hotspot hotspot)
                {
                    hotspot.Camera.transform.position = hit.point + new Vector3(0, 1.5f, 0);
                    hotspot.Camera.transform.localRotation = Quaternion.identity;
                }
                else if (obj is VideoRenderer videoRenderer)
                {
                    videoRenderer.Hitbox.transform.localScale = new Vector3(1, 1, 0.01f);
                }
            }
            else
            {
                SceneView.lastActiveSceneView.LookAt(obj.transform.position, camera.transform.rotation);
            }
#if UNITY_2020_3_OR_NEWER
            // Enable renaming mode
            Selection.activeGameObject = obj.gameObject;
            var type = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
            var hierarchyWindow = EditorWindow.GetWindow(type);
            var rename = type.GetMethod("FrameAndRenameNewGameObject", BindingFlags.Static | BindingFlags.NonPublic);
            rename.Invoke(null, null);
#endif
            return obj;
        }
    }
}
