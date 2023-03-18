using UnityEditor;

namespace Treasured.UnitySdk
{
    [InitializeOnLoad]
    public static class SessionKeys
    {
        //TreasuredSDK_
        public static readonly string ShowInteractableButtonFoldout = "TreasuredSDK_InteractableButtonFoldout";
        public static readonly string ShowActionList = "TreasuredSDK_ActionListExpanded";
        public static readonly string CustomIconsFolder = "TreasuredSDK_CustomIconsFolderKey";

        public static readonly string ShowDeprecatedActions = "TreasuredSDK_ShowDeprecatedActions";

        public static readonly string CLIProcessId = "TreasuredSDK_CLIProcessId";
    }
}
