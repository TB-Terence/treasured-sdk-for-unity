using System.IO;
using UnityEngine;

namespace Treasured.UnitySdk
{
    internal sealed class TreasuredMapEditorSettings : ScriptableObject
    {
        [SerializeField]
        [Header("General")]
        [Indent(1)]
        private TreasuredMap startUpAsset;

        [Header("Gizmos")]
        [SerializeField]
        [Indent(1)]
        public Color cameraColor = Color.red;
        [SerializeField]
        [Indent(1)]
        public Color hitboxColor = Color.green;
        [SerializeField]
        [Indent(1)]
        public Color pathColor = Color.white;
        public TreasuredMap StartUpAsset => startUpAsset;
    }
}
