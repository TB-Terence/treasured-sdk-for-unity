using UnityEngine;

namespace Treasured.UnitySdk
{
    /// <summary>
    /// Interactable object in the scene. Usually frames or 3D models.
    /// </summary>
    [AddComponentMenu("Treasured/Interactable")]
    public sealed class Interactable : TreasuredObject
    {
        #region Editor GUI Functions
#if UNITY_EDITOR
        void OnSceneViewFocus()
        {
            // Always oppsite to the transform.forward
            Vector3 targetPosition = Hitbox.transform.position;
            Vector3 cameraPosition = Hitbox.transform.position + Hitbox.transform.forward * 1;
            UnityEditor.SceneView.lastActiveSceneView.LookAt(cameraPosition, Quaternion.LookRotation(targetPosition - cameraPosition), 1);
        }
#endif
        #endregion
    }
}
