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

#if UNITY_STANDALONE_WIN
            process.StartInfo.Arguments = $"{_baseArgument} {arguments}";
#elif UNITY_STANDALONE_OSX
            process.StartInfo.Arguments = $"{_baseArgument} '{arguments}'";
#endif
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.WorkingDirectory =
                TreasuredSDKPreferences.Instance.customExportFolder;

            return process;
        }

        public static void KillProcess(ref Process process)
        {
            // TODO: This might kill the new process with same handle after domain reload.
            // Kill the process

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
#if UNITY_STANDALONE_WIN
                FileName = "cmd.exe",
                Arguments = $"/C taskkill /pid {process.Id} /f /t",
                CreateNoWindow = true,
#elif UNITY_STANDALONE_OSX
            startInfo.FileName = "pkill";
            startInfo.Arguments = $"-P {pid}";
#endif
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            Process p = Process.Start(startInfo);
            process.WaitForExit();

            if (!process.HasExited)
            {
                process.Kill();
            }

            process = null;
        }
    }
}
