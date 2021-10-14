using System;
using UnityEngine;

namespace Treasured.UnitySdk
{
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
    }
}
