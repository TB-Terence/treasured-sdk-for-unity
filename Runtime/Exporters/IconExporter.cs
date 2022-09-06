using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Treasured.UnitySdk
{
    public class IconExporter : Exporter
    {
        public override DirectoryInfo CreateExportDirectoryInfo()
        {
            return Directory.CreateDirectory(Path.Combine(base.CreateExportDirectoryInfo().FullName, "icons"));
        }

        [UnityEngine.ContextMenu("Reset")]
        private void Reset()
        {
            enabled = true;
        }

        public override void Export()
        {
            HashSet<string> iconNames = new HashSet<string>();
            string iconDirectory = CreateExportDirectoryInfo().FullName;
            foreach (var obj in Map.GetComponentsInChildren<TreasuredObject>())
            {
                if (obj.button == null || obj.button.asset == null || string.IsNullOrWhiteSpace(obj.button.asset.svg))
                {
                    continue;
                }
                // TODO : Validate XML file
                if (iconNames.Contains(obj.button.asset.name))
                {
                    continue;
                }
                File.WriteAllText(Path.Combine(iconDirectory, $"{obj.button.asset.name}.svg").ToOSSpecificPath(), obj.button.asset.svg);
                iconNames.Add(obj.button.asset.name);
            }
        }
    }
}
