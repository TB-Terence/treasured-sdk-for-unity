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
        private static readonly string _processName = "cmd.exe";

        public static readonly string TreasuredPluginsFolder =
 Path.GetFullPath("Packages/com.treasured.unitysdk/Plugins/Win").ToOSSpecificPath();
        
        private static readonly string _baseArgument = "/C";

#elif UNITY_STANDALONE_OSX

        private static readonly string _processName = "zsh";

        public static readonly string TreasuredPluginsFolder =
            Path.GetFullPath("Packages/com.treasured.unitysdk/Plugins/OSX").ToOSSpecificPath();

        private static readonly string _baseArgument = "-i -c";
#endif

        public static Process CreateProcess(string arguments)
        {
            Process process = new Process();
            process.StartInfo.FileName = _processName;
            process.StartInfo.Arguments = $"{_baseArgument} '{arguments}'";

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