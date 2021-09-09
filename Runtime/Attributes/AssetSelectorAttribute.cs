using UnityEngine;

namespace Treasured.UnitySdk
{
    public class AssetSelectorAttribute : PropertyAttribute
    {
        public bool IsFolder { get; set; }

        public AssetSelectorAttribute(bool isFolder = false)
        {
            IsFolder = isFolder;
        }
    }
}