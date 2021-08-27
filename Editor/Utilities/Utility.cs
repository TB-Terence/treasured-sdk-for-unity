using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.SceneManagement;
using System.Linq;

namespace Treasured.SDKEditor
{
    internal static class Utility
    {
        public static T ShowCreateAssetPanel<T>(string fileName, string defaultFolderIfInvalid) where T : ScriptableObject
        {
            string filePath = EditorUtility.SaveFilePanel("Choose folder", Application.dataPath, fileName, "asset");
            if (string.IsNullOrEmpty(filePath))
            {
                return null;
            }
            FileInfo fileInfo = new FileInfo(filePath);
            bool insideAssetFolder = filePath.StartsWith(Application.dataPath);
            T asset = ScriptableObject.CreateInstance<T>();
            if (!insideAssetFolder)
            {
                Debug.LogWarning($"Looks like the folder you choose is outside the Assets folder. The asset will be saved in {{{defaultFolderIfInvalid}}}.");
            }
            else
            {
                filePath = filePath.Substring(Application.dataPath.Length - 6);
            }
            if (!insideAssetFolder || fileInfo.DirectoryName.Equals("Assets"))
            {
                Directory.CreateDirectory(defaultFolderIfInvalid);
            }
            filePath = AssetDatabase.GenerateUniqueAssetPath(filePath);
            AssetDatabase.CreateAsset(asset, filePath);
            AssetDatabase.SaveAssets();
            return asset;
        }

        /// <summary>
        /// Return false when selection is outside of the 'Asset' folder or it was cancelled.
        /// </summary>
        /// <param name="guid">The guid of the asset to set.</param>
        /// <param name="path">The path of the asset to set.</param>
        /// <param name="directory">The directory to open when open the panel.</param>
        /// <param name="isFolderAsset">Is the asset a folder asset?</param>
        /// <returns></returns>
        public static bool OpenSelectAssetPanel(ref string guid, ref string path, string directory, bool isFolderAsset)
        {
            string selectedPath = isFolderAsset ? EditorUtility.OpenFolderPanel("Choose folder", directory, "") : EditorUtility.OpenFilePanel("Choose asset", directory, "asset");
            if (string.IsNullOrEmpty(selectedPath) || !selectedPath.StartsWith(Application.dataPath) || !selectedPath.EndsWith(".asset"))
            {
                return false;
            }
            selectedPath = selectedPath.Substring(Application.dataPath.Length - 6);
            guid = AssetDatabase.AssetPathToGUID(selectedPath);
            path = selectedPath;
            return true;
        }

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
