using UnityEngine;

namespace Treasured.UnitySdk
{
    [API("group")]
    public class GroupAction : ScriptableAction
    {
        public ScriptableActionCollection actions;

        public GroupAction()
        {
            actions = ScriptableObject.CreateInstance<ScriptableActionCollection>();
        }
    }
}
