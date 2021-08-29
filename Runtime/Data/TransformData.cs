using System;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [Serializable]
    public class TransformData
    {
        [SerializeField]
        private Vector3 _position;
        [SerializeField]
        private Vector3 _rotation;

        public Vector3 Position { get => _position; set => _position = value; }
        public Vector3 Rotation { get => _rotation; set => _rotation = value; }
        
        public static implicit operator TransformData(Transform transform)
        {
            return new TransformData()
            {
                _position = transform.position,
                _rotation = transform.eulerAngles
            };
        }
    }
}
