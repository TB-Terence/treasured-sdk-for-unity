using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace Treasured.UnitySdk
{
    internal class ProcessUtilities
    {
#if UNITY_STANDALONE_WIN
        private static string processName = "cmd.exe";

        public static readonly string TreasuredPluginsFolder =
 Path.GetFullPath("Packages/com.treasured.unitysdk/Plugins/Win").ToOSSpecificPath();

#elif UNITY_STANDALONE_OSX

        private static string processName = "zsh";

        public static readonly string TreasuredPluginsFolder =
            Path.GetFullPath("Packages/com.treasured.unitysdk/Plugins/OSX").ToOSSpecificPath();
#endif

        public static Process CreateProcess(string arguments)
        {
            Process process = new Process();
            process.StartInfo.FileName = processName;
            process.StartInfo.Arguments = $"-i -c '{arguments}'";

            process.StartInfo.CreateNoWindow = false;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.WorkingDirectory =
                TreasuredSDKPreferences.Instance.customExportFolder;

            return process;
        }
    }
}