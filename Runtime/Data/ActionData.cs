using System;

namespace Treasured.UnitySdk
{
    [Serializable]
    public abstract class ActionData
    {
    }

    public class NavigateAction : ActionData
    {
        public Hotspot target;
    }
}
