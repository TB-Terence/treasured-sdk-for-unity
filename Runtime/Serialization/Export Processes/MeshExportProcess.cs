using UnityEngine;

namespace Treasured.UnitySdk.Export
{
    public class MeshExportProcess : ExportProcess
    {
        [SerializeField]
        private bool _canUseTag;

        public bool CanUseTag
        {
            get => _canUseTag;
            set => _canUseTag = value;
        }

        [SerializeField]
        private string _filterTag = "Untagged";

        public string FilterTag
        {
            get => _filterTag;
            set => _filterTag = value;
        }

        [SerializeField]
        private bool _canUseLayerMask;

        public bool CanUseLayerMask
        {
            get => _canUseLayerMask;
            set => _canUseLayerMask = value;
        }

        [SerializeField]
        private LayerMask _filterLayerMask;

        public LayerMask FilterLayerMask
        {
            get => _filterLayerMask;
            set => _filterLayerMask = value;
        }

        [SerializeField]
        [Tooltip("Keep combined mesh gameObject in scene after exporting to GLB mesh")]
        private bool _keepCombinedMesh;

        public bool KeepCombinedMesh
        {
            get => _keepCombinedMesh;
            set => _keepCombinedMesh = value;
        }
    }
}
