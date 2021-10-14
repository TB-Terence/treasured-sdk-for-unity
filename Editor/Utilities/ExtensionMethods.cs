using UnityEngine;

namespace Treasured.UnitySdk.Editor
{
    internal static class ExtensionMethods
    {
        public static string GetFullPath(this Component component)
        {
            string path = component.gameObject.transform.name;
            if (component.gameObject.transform.parent == null)
            {
                return path;
            }
            return $"{GetFullPath(component.gameObject.transform.parent)}/{path}";
        }

        public static string GetRelativePath<T>(this Component component) where T : Component
        {
            string path = component.gameObject.transform.name;
            if (component.gameObject.transform.parent.GetComponent<T>())
            {
                return path;
            }
            return $"{GetRelativePath<T>(component.gameObject.transform.parent)}/{path}";
        }
    }
}
