using Newtonsoft.Json;
using System;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [Serializable]
    public struct TransformData
    {
        /// <summary>
        /// The position of the transform in world space.
        /// </summary>
        public Vector3 position;
        /// <summary>
        /// The euler angles of the transform in world space.
        /// </summary>
        public Vector3 eulerAngles;

        /// <summary>
        /// Rotation of the Transform(ReadOnly). This is same as Quaternion.Euler(eulerAngles).
        /// </summary>
        [JsonIgnore]
        public Quaternion Rotation
        {
            get
            {
                return Quaternion.Euler(eulerAngles);
            }
            set
            {
                eulerAngles = value.eulerAngles;
            }
        }

        public TransformData(Vector3 position, Vector3 eulerAngles) 
        {
            this.position = position;
            this.eulerAngles = eulerAngles;
        }

        public TransformData(Vector3 position, Quaternion rotation) : this(position, rotation.eulerAngles) { }
    }
}
