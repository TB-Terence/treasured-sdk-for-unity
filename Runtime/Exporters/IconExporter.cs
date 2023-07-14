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
            foreach (var obj in Scene.GetComponentsInChildren<TreasuredObject>())
            {
                if (obj.icon == null || obj.icon.asset == null || string.IsNullOrWhiteSpace(obj.icon.asset.svg))
                {
                    continue;
                }
                // TODO : Validate XML file
                if (iconNames.Contains(obj.icon.asset.name))
                {
                    continue;
                }
                File.WriteAllText(Path.Combine(iconDirectory, $"{obj.icon.asset.name}.svg").ToOSSpecificPath(), obj.icon.asset.svg);
                iconNames.Add(obj.icon.asset.name);
            }
        }
    }
}
