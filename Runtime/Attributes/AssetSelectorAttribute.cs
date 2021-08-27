using UnityEngine;

namespace Treasured.SDK
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