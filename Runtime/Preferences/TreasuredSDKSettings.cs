using UnityEngine;

namespace Treasured.UnitySdk
{
    internal class TreasuredSDKSettings : ScriptableObject
    {
        public static readonly Color DefaultFrustumColor = Color.red;
        public static readonly Color DefaultHitboxColor = new Color(0, 1, 0, 0.2f);

        [Header("Gizmos")]
        [Tooltip("Auto focus on Treasured Object when being selected.")]
        public bool autoFocus = true;
        [Tooltip("Gizmos color for hotspot camera")]
        public Color frustumColor = DefaultFrustumColor;
        [Tooltip("Gizmos color for hitbox")]
        public Color hitboxColor = DefaultHitboxColor;
    }
}
