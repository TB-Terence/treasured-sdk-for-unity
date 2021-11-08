using UnityEngine;
using UnityEditor;

namespace Treasured.UnitySdk
{
    [InitializeOnLoad]
    internal static class EditorCallbackRegister
    {
        static EditorCallbackRegister()
        {
            Selection.selectionChanged += () =>
            {
                var to = Selection.activeGameObject?.GetComponent<TreasuredObject>();
                if (to && TreasuredSDKSettings.Instance.autoFocus)
                {
                    to.TryInvokeMethods("OnSceneViewFocus");
                }
            };
        }
    }
}
