using System;

namespace Treasured.UnitySdk
{   
    public class ShowPreviewAction : Action
    {
        public TreasuredObject target;

        internal override ScriptableAction ConvertToScriptableAction()
        {
            Actions.ShowPreviewAction action = new Actions.ShowPreviewAction();
            action.target = target;
            return action;
        }
    }
}
