using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Treasured.UnitySdk
{
    public class IconExporter : Exporter
    {
        public override DirectoryInfo CreateExprtDirectoryInfo()
        {
            return Directory.CreateDirectory(Path.Combine(base.CreateExprtDirectoryInfo().FullName, "icons"));
        }

        public override void Export()
        {
            HashSet<string> iconNames = new HashSet<string>();
            string iconDirectory = CreateExprtDirectoryInfo().FullName;
            foreach (var obj in Map.GetComponentsInChildren<TreasuredObject>())
            {
                if (obj.button == null || obj.button.icon == null || string.IsNullOrWhiteSpace(obj.button.icon.svg))
                {
                    continue;
                }
                // TODO : Validate XML file
                if (iconNames.Contains(obj.button.icon.name))
                {
                    continue;
                }
                File.WriteAllText(Path.Combine(iconDirectory, $"{obj.button.icon.name}.svg"), obj.button.icon.svg);
                iconNames.Add(obj.button.icon.name);
            }
        }
    }
}
