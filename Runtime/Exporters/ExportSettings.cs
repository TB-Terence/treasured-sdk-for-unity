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
        public static string ExportRoot
        {
            get; set;
        }

        [RequiredField]
        public string folderName;
        [Tooltip("Optimize the cubemaps and the scene.glb to decrease the final file size.Keep in mind that this will result in a longer export process.")]
        public bool optimizeScene = true;
        public Thumbnail thumbnail;

        public string OutputDirectory
        {
            get
            {
                if (string.IsNullOrWhiteSpace(folderName))
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
