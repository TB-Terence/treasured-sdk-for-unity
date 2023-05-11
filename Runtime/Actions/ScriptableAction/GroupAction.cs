using UnityEngine;

namespace Treasured.UnitySdk
{
    [API("group")]
    public class GroupAction : ScriptableAction
    {
        public ScriptableActionCollection actions;
        //public ActionCollection actions;

        private void OnEnable()
        {
           // actions = ScriptableObject.CreateInstance<ScriptableActionCollection>();
        }
    }
}
