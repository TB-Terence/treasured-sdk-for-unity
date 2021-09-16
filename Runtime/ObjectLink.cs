using UnityEngine;

namespace Treasured.UnitySdk
{
    [DisallowMultipleComponent]
    public class ObjectLink : MonoBehaviour
    {
        [SerializeField]
        internal TreasuredMap _map;
        [SerializeField]
        internal string _targetId;
    }
}
