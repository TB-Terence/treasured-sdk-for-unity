using UnityEditor;

namespace Treasured.UnitySdk
{
    public static class ScriptingDefineSymbolsUtils
    {
        public static void AddDefineSymbol(string define)
        {
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
            if (!defines.Contains(define))
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, $"{defines};{define}");
            }
        }

        public static void RemoveDefineSymbol(string define)
        {
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
            if (defines.Contains(define))
            {
                string newDefines = defines.Replace($";{define}", "");
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, newDefines);
            }
        }

        public static string[] GetDefineSymbols()
        {
            return PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone).Split(',');
        }
    }
}
