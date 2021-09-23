using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk.Editor
{
    internal static class Utilities
    {
        public static void MoveSceneViewAndSelect(this Transform transform, float newSize = 0)
        {
            MoveSceneView(transform, newSize);
            Selection.activeGameObject = transform.gameObject;
        }

        public static void MoveSceneView(this Transform transform, float newSize = 0)
        {
            if (newSize == 0)
            {
                SceneView.lastActiveSceneView.LookAt(transform.position, transform.rotation, 0.01f);
            }
            else
            {
                Vector3 targetPosition = transform.position;
                Vector3 cameraPosition = transform.position + transform.forward * newSize;
                SceneView.lastActiveSceneView.LookAt(cameraPosition, Quaternion.LookRotation(targetPosition - cameraPosition), newSize);
            }
        }

        public static Transform GetPreviousSibling(this Transform transform)
        {
            if (transform == null)
            {
                Debug.LogWarning("Can not get previous sibling. The transform is null");
                return null;
            }
            if (transform.parent == null)
            {
                Transform[] rootTransforms = transform.gameObject.scene.GetRootGameObjects().Select(x => x.transform).ToArray();
                int rootIndex = transform.GetSiblingIndex();
                if (rootIndex == 0 && rootTransforms.Length > 1)
                {
                    return rootTransforms[rootTransforms.Length - 1];
                }
                return rootTransforms[rootIndex - 1];
            }
            int childCount = transform.parent.childCount;
            int index = transform.GetSiblingIndex();
            if (index == 0 && childCount > 1)
            {
                return transform.parent.GetChild(childCount - 1);
            }
            return transform.parent.GetChild(index - 1);
        }

        public static Transform GetNextSibling(this Transform transform)
        {
            if (transform == null)
            {
                Debug.LogWarning("Can not get next sibling. The transform is null");
                return null;
            }
            if (transform.parent == null)
            {
                Transform[] rootTransforms = transform.gameObject.scene.GetRootGameObjects().Select(x => x.transform).ToArray();
                int rootIndex = transform.GetSiblingIndex();
                if (rootIndex == rootTransforms.Length - 1 && rootTransforms.Length > 1)
                {
                    return rootTransforms[0];
                }
                return rootTransforms[rootIndex + 1];
            }
            int childCount = transform.parent.childCount;
            int index = transform.GetSiblingIndex();
            if (index == childCount - 1 && childCount > 1)
            {
                return transform.parent.GetChild(0);
            }
            return transform.parent.GetChild(index + 1);
        }
    }
}
