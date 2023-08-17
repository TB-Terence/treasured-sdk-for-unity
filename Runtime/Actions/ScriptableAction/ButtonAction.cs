using UnityEngine;

namespace Treasured.UnitySdk
{
    [API("button")]
    public class ButtonAction : ScriptableAction
    {
        [TextArea]
        public string text = "Continue?";
        public string buttonText = "Yes";
    }
}
