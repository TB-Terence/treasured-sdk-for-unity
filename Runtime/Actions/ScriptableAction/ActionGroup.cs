using System;
using UnityEngine;

namespace Treasured.UnitySdk.Actions
{
    [Serializable]
    public class ActionGroup
    {
        public ScriptableActionCollection onClick;
        public ScriptableActionCollection onHover;
    }
}
