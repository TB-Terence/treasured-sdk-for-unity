using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    public class ExportSettings : ScriptableObject
    {
#if UNITY_EDITOR
        public const string OutputRootDirectoryKey = "TreasuredSDK_Output_Root_Directory";
#endif
        /// <summary>
        /// Default output folder in project root
        /// </summary>
        public const string DefaultOutputFolderName = "Treasured Data/";
        public static readonly string DefaultOutputDirectory = $"{Directory.GetCurrentDirectory()}/{DefaultOutputFolderName}";
        public static string ExportRoot
        {
            get; set;
        }

        public string GetOutputDirecotry()
        {
#if UNITY_EDITOR
            return UnityEditor.EditorPrefs.GetString(OutputRootDirectoryKey, DefaultOutputFolderName);
#else
            return Path.Combine(ExportRoot, folderName);
#endif
        }

        [RequiredField]
        public string folderName;

        public bool ignoreWarnings = false;

        public string OutputDirectory
        {
            get
            {
                if (string.IsNullOrEmpty(folderName))
                {
                    return "";
                }
                return Path.Combine(GetOutputDirecotry(), folderName);
            }
        }
    }
}
