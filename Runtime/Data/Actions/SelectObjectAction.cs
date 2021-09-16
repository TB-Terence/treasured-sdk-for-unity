using UnityEngine;

namespace Treasured.UnitySdk
{
    public class SelectObjectAction : ActionBase
    {
        [SerializeField]
        private string _targetId;

        /// <summary>
        /// Id of the object to select.
        /// </summary>
        public string TargetId => _targetId;
    }
}
