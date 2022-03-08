using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [ExportProcessSettings()]
    [System.Serializable]
    internal abstract class ExportProcess
    {
        /// <summary>
        /// Default output folder in project root
        /// </summary>
        public const string DefaultOutputFolder = "Treasured Data/";
        public static readonly string DefaultOutputFolderPath = $"{Directory.GetCurrentDirectory()}/{DefaultOutputFolder}";

        [HideInInspector]
        public bool enabled;

        public virtual void OnEnable(SerializedObject serializedObject) { }
        public virtual void OnGUI(string root, SerializedObject map) { }
        public virtual void OnPreferenceGUI(SerializedObject settings) { }
        public virtual void OnPreProcess(SerializedObject serializedObject) { }
        public abstract void OnExport(string rootDirectory, TreasuredMap map);
        public virtual void OnPostProcess(SerializedObject serializedObject) { }
    }
}
