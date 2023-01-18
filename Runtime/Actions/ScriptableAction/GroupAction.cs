using UnityEngine;

namespace Treasured.UnitySdk
{
    [API("group")]
    public class GroupAction : ScriptableAction
    {
        public ScriptableActionCollection actions;

        void OnEnable()
        {
            actions ??= ScriptableObject.CreateInstance<ScriptableActionCollection>();
        }
    }
}
