using System;
using System.IO;
using UnityEngine;

namespace Treasured.UnitySdk
{
    public class ExportSettings : ScriptableObject
    {
        /// <summary>
        /// Default output folder in project root
        /// </summary>
        public const string DefaultOutputFolderName = "Treasured Data/";
        public static readonly string DefaultOutputDirectory = $"{Directory.GetCurrentDirectory()}/{DefaultOutputFolderName}";
        public static string CustomOutputDirectory = "";

        public static string GetOutputDirecotry()
        {
            if (string.IsNullOrEmpty(CustomOutputDirectory))
            {
                return DefaultOutputDirectory;
            }
            return CustomOutputDirectory;
        }

        public string folderName;

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
