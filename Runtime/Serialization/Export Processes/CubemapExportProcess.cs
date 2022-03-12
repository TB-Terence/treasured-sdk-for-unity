using UnityEngine;

namespace Treasured.UnitySdk.Export
{
    public class CubemapExportProcess : ExportProcess
    {
        public const int MAXIMUM_CUDA_TEXTURE_WIDTH = 4096;
        public const int MAXIMUM_CUBEMAP_FACE_WIDTH = 1360; // 4096 / 3 round to nearest tenth
        public const int MAXIMUM_CUBEMAP_WIDTH = 8192;

        public ImageFormat imageFormat = ImageFormat.Ktx2;
        public ImageQuality imageQuality = ImageQuality.High;
        public CubemapFormat cubemapFormat = CubemapFormat.IndividualFace;
        public bool flipY = true;

        [SerializeField]
        [HideInInspector]
        private int _qualityPercentage = 75;
    }
}
