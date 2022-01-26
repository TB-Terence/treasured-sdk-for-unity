using UnityEngine;

namespace Treasured.UnitySdk
{
    public class VideoPlane : TreasuredObject
    {
        #region
        [SerializeField]
        private int _width;
        [SerializeField]
        private int _height;
        [SerializeField]
        private float _aspectRatio;
        [SerializeField]
        private string _src;
        #endregion

        public int Width => _width;
        public int Height => _height;
        public float AspectRatio => _aspectRatio;
        public string Src => _src;

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            Color tempColor = Gizmos.color;
            Matrix4x4 tempMatrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(this.transform.position, this.transform.rotation, Vector3.one);
            Gizmos.color = Color.grey;
            Gizmos.DrawCube(Vector3.zero, new Vector3(Width / 1000f * transform.localScale.x, Height / 1000f * transform.localScale.y, 0));
            Gizmos.color = tempColor;
            Gizmos.matrix = tempMatrix;
        }
#endif
    }
}
