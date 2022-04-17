using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Treasured.UnitySdk
{
    [HideInInspector]
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
                if (obj.button == null || obj.button.iconAsset == null || string.IsNullOrWhiteSpace(obj.button.iconAsset.svg))
                {
                    continue;
                }
                // TODO : Validate XML file
                if (iconNames.Contains(obj.button.iconAsset.name))
                {
                    continue;
                }
                File.WriteAllText(Path.Combine(iconDirectory, $"{obj.button.iconAsset.name}.svg"), obj.button.iconAsset.svg);
                iconNames.Add(obj.button.iconAsset.name);
            }
        }
    }
}
