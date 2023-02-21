using System;
using System.IO;
using UnityEditor;
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
        public static string ExportRoot
        {
            get; set;
        }

        [RequiredField]
        public string folderName;
        public bool optimizeScene = true;

        public string OutputDirectory
        {
            get
            {
                if (string.IsNullOrEmpty(folderName))
                {
                    return "";
                }
                return Path.Combine(TreasuredSDKPreferences.Instance.customExportFolder, folderName).ToOSSpecificPath();
            }
        }

        public ExportType ExportType;
    }
    
    public enum ExportType
    {
        Export,
        ProductionExport,
    }
}
