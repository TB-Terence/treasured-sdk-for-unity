using UnityEngine;

namespace Treasured.UnitySdk
{
    internal static class ObjectExtensionMethods
    {
        public static bool IsNullOrNone(this UnityEngine.Object obj)
        {
            return obj is null || obj.GetInstanceID() == -1;
        }
    }
    internal static class GameObjectExtensionMethods
    {
        public static Transform FindOrCreateChild(this GameObject gameObject, string name)
        {
            Transform transform = gameObject.transform.Find(name);
            if (transform == null)
            {
                GameObject go = new GameObject(name);
                transform = go.transform;
                go.transform.SetParent(gameObject.transform);
            }
            return transform;
        }

        public static T FindOrCreateChild<T>(this GameObject gameObject, string name) where T : MonoBehaviour
        {
            T t = gameObject.GetComponentInChildren<T>();
            if (t == null)
            {
                GameObject go = new GameObject(name);
                t = go.AddComponent<T>();
                go.transform.SetParent(gameObject.transform);
            }
            return t;
        }
    }
}
