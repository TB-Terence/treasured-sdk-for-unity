using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    internal static class EditorUtilities
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

        private static MethodInfo setIconForObjectMethodInfo;
        private static MethodInfo getIconForObjectMethodInfo;

        public static readonly Texture2D dotGray = (Texture2D)EditorGUIUtility.IconContent("sv_icon_dot8_pix16_gizmo").image;


        private static void SetIconForObject(UnityEngine.Object obj, Texture2D icon)
        {
            if (setIconForObjectMethodInfo == null)
            {
                setIconForObjectMethodInfo = typeof(EditorGUIUtility).GetMethod("SetIconForObject", BindingFlags.Static | BindingFlags.NonPublic);
            }
            setIconForObjectMethodInfo?.Invoke(null, new object[] { obj, icon });
        }

        public static void SetIcon(this GameObject go, Texture2D icon)
        {
            SetIconForObject(go, icon);
        }

        public static void SetLabelIcon(this GameObject go, int index)
        {
            index = Mathf.Clamp(index, 0, 7);
            SetIconForObject(go, (Texture2D)EditorGUIUtility.IconContent($"sv_label_{index}").image);
        }

        public static void SetDotIcon(this GameObject go, int index)
        {
            index = Mathf.Clamp(index, 0, 15);
            SetIconForObject(go, (Texture2D)EditorGUIUtility.IconContent($"sv_icon_dot{index}_pix16_gizmo").image);
        }

        public static Texture2D GetIcon(this GameObject go)
        {
            if (getIconForObjectMethodInfo == null)
            {
                getIconForObjectMethodInfo = typeof(EditorGUIUtility).GetMethod("GetIconForObject", BindingFlags.Static | BindingFlags.NonPublic);
            }
            return (Texture2D)(getIconForObjectMethodInfo?.Invoke(null, new object[] { go }));
        }

#if UNITY_2020_3_OR_NEWER
        public static void FrameAndRenameNewGameObject()
        {
            // Enable renaming mode
            var sceneHierarchyWindowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
            var hierarchyWindow = EditorWindow.GetWindow(sceneHierarchyWindowType);
            var rename = sceneHierarchyWindowType.GetMethod("FrameAndRenameNewGameObject", BindingFlags.Static | BindingFlags.NonPublic);
            rename.Invoke(null, null);
        }

        public static void RenameGO(GameObject go)
        {
            var sceneHierarchyWindowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
            if (sceneHierarchyWindowType == null)
            {
                return;
            }
            var sceneHierarchyPI = sceneHierarchyWindowType.GetProperty("sceneHierarchy", BindingFlags.Public | BindingFlags.Instance);
            if (sceneHierarchyPI == null)
            {
                return;
            }
            var treeViewPI = sceneHierarchyPI.PropertyType.GetProperty("treeView", BindingFlags.NonPublic | BindingFlags.Instance);
            if (treeViewPI == null)
            {
                return;
            }
            var setSelectionMI = treeViewPI.PropertyType.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(method => method.Name == "SetSelection" && method.GetParameters().Length == 2).First();
            if (setSelectionMI == null)
            {
                return;
            }
            var beginNameEditingMI = treeViewPI.PropertyType.GetMethod("BeginNameEditing", BindingFlags.Public | BindingFlags.Instance);
            if (beginNameEditingMI == null)
            {
                return;
            }
            var window = EditorWindow.GetWindow(sceneHierarchyWindowType);
            if (window == null)
            {
                return;
            }
            var sceneHierarchy = sceneHierarchyPI.GetValue(window);
            if (sceneHierarchy == null)
            {
                return;
            }
            var treeView = treeViewPI.GetValue(sceneHierarchy);
            setSelectionMI.Invoke(treeView, new object[] { new int[] { go.GetInstanceID() }, true });
            beginNameEditingMI.Invoke(treeView, new object[] { 0 });
        }
#endif
    }
}
