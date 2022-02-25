using System.IO;
using UnityEditor;

namespace Treasured.UnitySdk
{
    [ExportProcessSettings()]
    internal abstract class ExportProcess
    {
        /// <summary>
        /// Default output folder in project root
        /// </summary>
        public const string DefaultOutputFolder = "Treasured Data/";
        public static readonly string DefaultOutputFolderPath = $"{Directory.GetCurrentDirectory()}/{DefaultOutputFolder}";

        public virtual void OnEnable(SerializedObject serializedObject) { }
        public virtual void OnGUI(SerializedObject serializedObject) { }
        public virtual void OnPreProcess(SerializedObject serializedObject) { }
        public abstract void OnExport(string rootDirectory, TreasuredMap map);
        public virtual void OnPostProcess(SerializedObject serializedObject) { }
    }
}
