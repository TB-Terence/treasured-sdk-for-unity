using System;
using UnityEngine;

namespace Treasured.SDK
{
    [DisallowMultipleComponent]
    [ExecuteInEditMode]
    public class TreasuredObjectReference : MonoBehaviour
    {
        [SerializeField]
        [ReadOnly]
        private string _id;

        public string Id { get => _id; }

        private void Awake()
        {
            if(string.IsNullOrEmpty(Id))
            {
                _id = Guid.NewGuid().ToString();
            }
            hideFlags = HideFlags.DontSaveInBuild;
        }
    }
}
