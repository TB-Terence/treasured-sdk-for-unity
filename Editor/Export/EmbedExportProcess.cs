using UnityEngine;
using UnityEditor;
using System.IO;

namespace Treasured.UnitySdk
{
    internal class EmbedExportProcess : ExportProcess
    {
        private string output;

        public override void OnGUI(SerializedObject serializedObject)
        {
            output = EditorGUILayout.TextArea(output);
        }

        public override void OnExport(string rootDirectory, TreasuredMap map)
        {
            string path = Path.Combine(rootDirectory, "embed.html");
            HTMLGenerator.Generate(path, output);
        }
    }
}
