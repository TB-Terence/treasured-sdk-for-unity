using System.IO;
using UnityEditor;

namespace Treasured.UnitySdk
{
    internal abstract class ExportProcess
    {
        /// <summary>
        /// Default output folder in project root
        /// </summary>
        public const string DefaultOutputFolder = "Treasured Data/";
        public static readonly string DefaultOutputFolderPath = $"{Directory.GetCurrentDirectory()}/{DefaultOutputFolder}";

        public virtual bool Enabled { get; set; } = true;
        public virtual string DisplayName
        {
            get
            {
                string typeName = GetType().Name;
                if (typeName.EndsWith("ExportProcess"))
                {
                    typeName = typeName.Substring(0, typeName.Length - 13);
                }
                return ObjectNames.NicifyVariableName(typeName);
            }
        }
        public bool Expanded { get; set; } = true;
        public virtual void OnEnable(SerializedObject serializedObject) { }
        public virtual void OnGUI(SerializedObject serializedObject) { }
        public virtual void OnPreProcess(SerializedObject serializedObject) { }
        public abstract void OnExport(string rootDirectory, TreasuredMap map);
        public virtual void OnPostProcess(SerializedObject serializedObject) { }
    }
}
