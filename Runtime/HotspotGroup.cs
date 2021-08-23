using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Treasured.SDK
{
    [DisallowMultipleComponent]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    public class HotspotGroup : MonoBehaviour
    {
        [SerializeField]
        private Hotspot[] _hotspots;

        private void OnEnable()
        {
            _hotspots = gameObject.GetComponentsInChildren<Hotspot>();
        }
    }
}
