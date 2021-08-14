using System;
using UnityEngine;

namespace Treasured.SDK
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
    }
}
