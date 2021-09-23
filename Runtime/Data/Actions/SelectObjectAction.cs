using UnityEngine;

namespace Treasured.UnitySdk
{
    public class SelectObjectAction : ActionBase
    {
        [SerializeField]
        private TreasuredObject target;

        /// <summary>
        /// Id of the object to select.
        /// </summary>
        public string TargetId
        {
            get
            {
                if (target == null)
                {
                    return string.Empty;
                }
                return target.Id;
            }
        }
    }
}
